using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class DocumentService : IDocumentService
    {
        private IDocumentDb documentDb;
        private INavigationService navigationService;

        public DocumentService(IDocumentDb documentDb, INavigationService navigationService)
        {
            this.documentDb = documentDb;
            this.navigationService = navigationService;
        }

        private async Task ClearAllData()
        {
            await documentDb.RemoveAllDocumentsAsync();
        }

        private async Task InsertTestData()
        {
            var service = new MyDocs.WindowsStore.Service.Design.DesignDocumentService();
            var testCategories = await service.LoadAsync();

            foreach (var document in testCategories.SelectMany(c => c.Documents)) {
                await SaveDocumentAsync(document);
            }
        }

        public async Task<ImmutableList<Category>> LoadAsync()
        {
            // TODO create CachedDocumentService
            //if (Documents.Count > 0) {
            //    return;
            //}
#if DEBUG
            //await ClearAllData();
            //await InsertTestData();
            //Documents.Clear();
#endif
            //await Task.Delay(2000);
            //categories.Clear();

            var documents = await documentDb.GetAllDocumentsAsync();
            return documents.GroupBy(d => d.Category)
                .Select(g => new Category(g.Key, g))
                .ToImmutableList();

            // TODO create CleanDocumentService
            //await RemoveOutdatedDocuments();

            // TODO create CleanDocumentService
            //if (!navigationService.CanGoBack) {
            //    await ClearTempState();
            //}
        }

        //private async Task RemoveOutdatedDocuments()
        //{
        //    var documents = (from doc in Documents
        //                     where doc.HasLimitedLifespan
        //                     where doc.DateRemoved < DateTime.Today
        //                     select doc).ToList();

        //    foreach (var document in documents) {
        //        await DeleteDocumentAsync(document);
        //    }
        //}

        //private async Task ClearTempState()
        //{
        //    var files = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();
        //    foreach (var file in files) {
        //        await file.DeleteAsync();
        //    }
        //}

        public IEnumerable<string> GetCategoryNames()
        {
            return documentDb.GetDistinctCategories();
        }

        public IEnumerable<int> GetDistinctDocumentYears()
        {
            return documentDb.GetDistinctDocumentYears();
        }

        public async Task DeleteCategoryAsync(string categoryName)
        {
            var documents = await documentDb.GetDocuments(categoryName);
            await Task.WhenAll(documents.Select(DeleteDocumentAsync));
        }

        public async Task RenameCategoryAsync(string oldName, string newName)
        {
            if (oldName == newName) {
                return;
            }

            var documents = await documentDb.GetDocuments(oldName);
            await Task.WhenAll(SetDocumentsCategory(documents, newName));
        }

        private IEnumerable<Task> SetDocumentsCategory(IEnumerable<Document> documents, string categoryName) {
            foreach (var document in documents) {
                document.Category = categoryName;
                yield return SaveDocumentAsync(document);
            }
        }

        public async Task<Document> GetDocumentById(Guid id)
        {
            return await documentDb.GetDocument(id);
        }

        public async Task SaveDocumentAsync(Document document)
        {
            documentDb.Save(document);
            await Task.Yield();

            // TODO raise event
            //var existingDocument = Documents.SingleOrDefault(d => d.Id == document.Id);
            //if (existingDocument != null) {
            //    Documents.Remove(existingDocument);
            //}
            //Documents.Add(document);
        }

        public async Task DeleteDocumentAsync(Document document)
        {
            await RemovePhotosAsync(document.Photos);

            documentDb.Remove(document.Id.ToString());
            // TODO raise event
            //Documents.Remove(document);
        }

        public async Task RemovePhotosAsync(IEnumerable<Photo> photos)
        {
            await documentDb.RemovePhotosAsync(photos);
        }

        public event EventHandler Changed;
    }
}
