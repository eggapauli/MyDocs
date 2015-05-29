using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model.Logic;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class ApplicationDataContainerDocumentStorage : IDocumentDb
    {
        private static readonly string containerName = "docs";

        private readonly ISettingsService settingsService;
        private readonly IFolder tempFolder = new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder);

        private readonly IApplicationDataContainer docsDataContainer;

        public ApplicationDataContainerDocumentStorage(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            docsDataContainer = settingsService.SettingsContainer.CreateContainer(containerName);
        }

        public async Task ClearAllData()
        {
            docsDataContainer.Values.Clear();

            var folders = new[] {
                ApplicationData.Current.LocalFolder,
                ApplicationData.Current.RoamingFolder,
                ApplicationData.Current.TemporaryFolder
            };

            foreach (var folder in folders) {
                var files = await folder.GetFilesAsync();
                foreach (var file in files) {
                    await file.DeleteAsync();
                }
            }
        }

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            var convertTasks = docsDataContainer.Values.Values.Cast<ApplicationDataCompositeValue>()
                .Select(item => item.ConvertToDocumentAsync());

            return await Task.WhenAll(convertTasks);
        }

        public async Task<IEnumerable<Document>> GetDocuments(string categoryName)
        {
            var documents = await GetAllDocumentsAsync();
            return documents.Where(d => d.Category == categoryName);
        }

        public async Task<Document> GetDocument(Guid id)
        {
            var documents = await GetAllDocumentsAsync();
            return documents.SingleOrDefault(d => d.Id == id);
        }

        public async Task<IEnumerable<string>> GetDistinctCategories()
        {
            await Task.Yield();
            return (from item in docsDataContainer.Values.Values.Cast<ApplicationDataCompositeValue>()
                    let categoryName = (string)(item["Category"])
                    orderby categoryName
                    select categoryName).Distinct();
        }

        public async Task<IEnumerable<int>> GetDistinctDocumentYears()
        {
            await Task.Yield();
            return (from item in docsDataContainer.Values.Values.Cast<ApplicationDataCompositeValue>()
                    let yearAdded = ((DateTimeOffset)(item["DateAdded"])).Year
                    orderby yearAdded
                    select yearAdded).Distinct();
        }

        public async Task Save(Document document)
        {
            docsDataContainer.Values[document.Id.ToString()] = document.ConvertToStoredDocument();
            await Task.Yield();
        }

        public async Task Remove(Guid documentId)
        {
            docsDataContainer.Values.Remove(documentId.ToString());
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(documentId.ToString());
                await folder.DeleteAsync();
            }
            catch (FileNotFoundException) { }
        }
    }
}
