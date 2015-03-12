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
        private IPersistDocuments documentStorage;
        private INavigationService navigationService;

        private readonly ObservableCollection<Document> documents = new ObservableCollection<Document>();
        public ObservableCollection<Document> Documents
        {
            get { return documents; }
        }

        public DocumentService(IPersistDocuments documentStorage, INavigationService navigationService)
        {
            this.documentStorage = documentStorage;
            this.navigationService = navigationService;
        }

        private async Task ClearAllData()
        {
            await documentStorage.RemoveAllDocumentsAsync();
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

            foreach (var item in await documentStorage.GetAllDocumentsAsync()) {
                Documents.Add(item);
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
            return documentStorage.GetDistinctCategories();
        }

        public IEnumerable<int> GetDistinctDocumentYears()
        {
            return documentStorage.GetDistinctDocumentYears();
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
            await documentStorage.SaveAsync(document);

            var existingDocument = Documents.SingleOrDefault(d => d.Id == document.Id);
            if (existingDocument != null) {
                Documents.Remove(existingDocument);
            }
            Documents.Add(document);
        }

        public async Task DeleteDocumentAsync(Document document)
        {
            await RemovePhotosAsync(document.Photos);

            documentStorage.Remove(document.Id.ToString());
            Documents.Remove(document);
        }

        public async Task RemovePhotosAsync(IEnumerable<Photo> photos)
        {
            await documentStorage.RemovePhotosAsync(photos);
        }
    }
}
