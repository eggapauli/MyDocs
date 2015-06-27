using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyDocs.Common.Model.Logic;
using System.Collections.Immutable;
using System.Reactive.Linq;

namespace Common.Test.Mocks
{
    class DocumentServiceMock : IDocumentService
    {
        public readonly ImmutableList<Document> Documents;

        public event EventHandler Changed;
        
        public DocumentServiceMock(IEnumerable<Document> documents)
        {
            Documents = documents.ToImmutableList();
        }

        public Func<string, Task> DeleteCategoryAsyncFunc =
            async delegate { await Task.Yield(); };

        public Task DeleteCategoryAsync(string categoryName)
        {
            return DeleteCategoryAsyncFunc(categoryName);
        }

        public Func<Document, Task> DeleteDocumentAsyncFunc =
            async delegate { await Task.Yield(); };

        public Task DeleteDocumentAsync(Document doc)
        {
            return DeleteDocumentAsyncFunc(doc);
        }

        public IObservable<IEnumerable<string>> GetCategoryNames()
        {
            var categoryNames = Documents
                .Select(d => d.Category)
                .Distinct()
                .OrderBy(x => x)
                .AsEnumerable();
            return Observable.Return(categoryNames);
        }

        public IObservable<IEnumerable<int>> GetDistinctDocumentYears()
        {
            var years = Documents
                .Select(d => d.DateAdded.Year)
                .Distinct()
                .OrderBy(x => x)
                .AsEnumerable();
            return Observable.Return(years);
        }

        public Task<Document> GetDocumentById(Guid id)
        {
            return Task.FromResult(Documents.Single(d => d.Id == id));
        }

        public IObservable<IEnumerable<Document>> GetDocuments()
        {
            return Observable.Return(Documents);
        }

        public async Task RemovePhotosAsync(IEnumerable<Photo> photos)
        {
            await Task.Yield();
        }

        public Func<string, string, Task> RenameCategoryAsyncFunc =
            async delegate { await Task.Yield(); };

        public Task RenameCategoryAsync(string oldName, string newName)
        {
            return RenameCategoryAsyncFunc(oldName, newName);
        }

        public async Task SaveDocumentAsync(Document doc)
        {
            await Task.Yield();
        }
    }
}
