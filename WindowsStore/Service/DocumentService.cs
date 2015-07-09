using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentDb documentDb;

        public DocumentService(IDocumentDb documentDb)
        {
            this.documentDb = documentDb;
        }

        private async Task ClearAllData()
        {
            await documentDb.ClearAllData();
        }

        private async Task InsertTestData()
        {
            var service = new Design.DesignDocumentService();
            var testDocuments = await service.GetDocuments().FirstAsync();
            
            foreach (var document in testDocuments) {
                await SaveDocumentAsync(document);
            }
        }

        public IObservable<IEnumerable<Document>> GetDocuments()
        {
            return ObserveDocuments(documentDb.GetAllDocumentsAsync);
        }

        public async Task RemoveOutdatedDocuments()
        {
            var documents = (from doc in await documentDb.GetAllDocumentsAsync()
                             where doc.HasLimitedLifespan
                             where doc.DateRemoved < DateTime.Today
                             select doc).ToList();

            foreach (var document in documents)
            {
                await DeleteDocumentAsync(document);
            }
        }

        //private async Task ClearTempState()
        //{
        //    var files = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();
        //    foreach (var file in files) {
        //        await file.DeleteAsync();
        //    }
        //}

        public IObservable<IEnumerable<string>> GetCategoryNames()
        {
            return ObserveDocuments(documentDb.GetDistinctCategories);
        }

        public IObservable<IEnumerable<int>> GetDistinctDocumentYears()
        {
            return ObserveDocuments(documentDb.GetDistinctDocumentYears);
        }

        private IObservable<T> ObserveDocuments<T>(Func<Task<T>> func)
        {
            return documentDb.Changed
                .StartWith(Unit.Default)
                .Select(_ => func())
                .Switch()
                .Replay(1)
                .RefCount();
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
                // TODO use https://github.com/AArnott/ImmutableObjectGraph ?
                var subDocuments = document.SubDocuments.Select(sd => new SubDocument(sd.Title, sd.File, sd.Photos.Select(p => new Photo(p.File))));
                var doc = new Document(document.Id, categoryName, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags, subDocuments);
                yield return SaveDocumentAsync(doc);
            }
        }

        public async Task<Document> GetDocumentById(Guid id)
        {
            return await documentDb.GetDocument(id);
        }

        public async Task SaveDocumentAsync(Document document)
        {
            await documentDb.Save(document);

            // TODO raise event
            //var existingDocument = Documents.SingleOrDefault(d => d.Id == document.Id);
            //if (existingDocument != null) {
            //    Documents.Remove(existingDocument);
            //}
            //Documents.Add(document);
        }

        public async Task DeleteDocumentAsync(Document document)
        {
            await documentDb.Remove(document.Id);
            // TODO raise event
            //Documents.Remove(document);
        }

        public event EventHandler Changed;


        public async Task RemovePhotosAsync(IEnumerable<Photo> photos)
        {
            var tasks = photos.Select(p => p.File.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask());
            await Task.WhenAll(tasks);
        }
    }
}
