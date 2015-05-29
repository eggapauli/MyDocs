using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyDocs.Common.Model.Logic;
using System.Collections.Immutable;

namespace MyDocs.Common.Test
{
    public class DummyDocumentService : IDocumentService
    {
        private readonly IImmutableList<Document> documents;

        public event EventHandler Changed;

        public DummyDocumentService(IEnumerable<Document> documents)
        {
            this.documents = documents.ToImmutableList();
        }

        public Task DeleteCategoryAsync(string categoryName)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDocumentAsync(Document doc)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetCategoryNames()
        {
            return Task.FromResult(documents.Select(d => d.Category).Distinct());
        }

        public Task<IEnumerable<int>> GetDistinctDocumentYears()
        {
            var years = documents
                .Select(d => d.DateAdded.Year)
                .Distinct()
                .OrderBy(year => year)
                .AsEnumerable();
            return Task.FromResult(years);
        }

        public Task<Document> GetDocumentById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IImmutableList<Document>> LoadAsync()
        {
            return Task.FromResult(documents);
        }

        public Task RemovePhotosAsync(IEnumerable<Photo> photos)
        {
            throw new NotImplementedException();
        }

        public Task RenameCategoryAsync(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public Task SaveDocumentAsync(Document doc)
        {
            throw new NotImplementedException();
        }
    }
}
