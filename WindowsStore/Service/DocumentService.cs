using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class DocumentService : IDocumentService
    {
        private ISettingsService settingsService;
        private INavigationService navigationService;
        private IApplicationDataContainer docsDataContainer;

        private readonly IFolder tempFolder = new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder);

        public ObservableCollection<Document> Documents { get; private set; }

        public DocumentService(ISettingsService settingsService, INavigationService navigationService)
        {
            this.settingsService = settingsService;
            this.navigationService = navigationService;

            Documents = new ObservableCollection<Document>();

            docsDataContainer = settingsService.SettingsContainer.CreateContainer("docs");
        }

        private async Task ClearAllData()
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

        private async Task InsertTestData()
        {
            var service = new MyDocs.WindowsStore.Service.Design.DesignDocumentService();
            await service.LoadDocumentsAsync();

            foreach (var document in service.Documents) {
                await SaveDocumentAsync(document);
            }
        }

        public async Task LoadDocumentsAsync()
        {
            if (Documents.Count > 0) {
                return;
            }
#if DEBUG
            //await ClearAllData();
            //await InsertTestData();
            //Documents.Clear();
#endif
            //await Task.Delay(2000);
            //categories.Clear();

            foreach (var item in docsDataContainer.Values.Values.Cast<ApplicationDataCompositeValue>()) {
                Documents.Add(await item.ConvertToDocumentAsync());
            }

            await RemoveOutdatedDocuments();

            if (!navigationService.CanGoBack) {
                await ClearTempState();
            }
        }

        private async Task RemoveOutdatedDocuments()
        {
            var documents = (from doc in Documents
                             where doc.HasLimitedLifespan
                             where doc.DateRemoved < DateTime.Today
                             select doc).ToList();

            foreach (var document in documents) {
                await DeleteDocumentAsync(document);
            }
        }

        private async Task ClearTempState()
        {
            var files = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();
            foreach (var file in files) {
                await file.DeleteAsync();
            }
        }

        public IEnumerable<string> GetCategoryNames()
        {
            // TODO use distinct?
            return from obj in docsDataContainer.Values.Values
                   let item = obj as ApplicationDataCompositeValue
                   let categoryName = (string)(item["Category"])
                   group item by categoryName into category
                   orderby category.Key
                   select category.Key;
        }

        public IEnumerable<int> GetDistinctDocumentYears()
        {
            return (from obj in docsDataContainer.Values.Values
                    let item = obj as ApplicationDataCompositeValue
                    let yearAdded = ((DateTimeOffset)(item["DateAdded"])).Year
                    orderby yearAdded
                    select yearAdded).Distinct();
        }

        public async Task RenameCategoryAsync(string oldName, string newName)
        {
            if (oldName == newName) {
                return;
            }

            foreach (var document in Documents.Where(d => d.Category == oldName).ToList()) {
                document.Category = newName;
                await SaveDocumentAsync(document);
            }
        }

        public async Task<Document> GetDocumentById(Guid id)
        {
            await LoadDocumentsAsync();
            return Documents.Single(d => d.Id == id);
        }

        public async Task SaveDocumentAsync(Document document)
        {
            foreach (var file in document.Photos.SelectMany(p => p.Files).Where(f => f.IsInFolder(tempFolder))) {
                string name = Path.GetRandomFileName() + Path.GetExtension(file.Path);
                await file.MoveAsync(settingsService.PhotoFolder, name);
            }

            docsDataContainer.Values[document.Id.ToString()] = document.ConvertToStoredDocument();

            var existingDocument = Documents.SingleOrDefault(d => d.Id == document.Id);
            if (existingDocument != null) {
                Documents.Remove(existingDocument);
            }
            Documents.Add(document);
        }

        public async Task DeleteDocumentAsync(Document document)
        {
            await RemovePhotosAsync(document.Photos);

            docsDataContainer.Values.Remove(document.Id.ToString());
            Documents.Remove(document);
        }

        public async Task RemovePhotosAsync(IEnumerable<Photo> photos)
        {
            foreach (var file in photos.SelectMany(p => p.Files).Where(f => !f.IsInFolder(tempFolder))) {
                await file.MoveAsync(tempFolder);
            }
        }
    }
}
