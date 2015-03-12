using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IDocumentService
    {
        event EventHandler Changed;

        Task<ImmutableList<Category>> LoadAsync();
        IEnumerable<string> GetCategoryNames();
        IEnumerable<int> GetDistinctDocumentYears();

        Task RenameCategoryAsync(string oldName, string newName);
        Task DeleteCategoryAsync(string categoryName);

        Task<Document> GetDocumentById(Guid id);
        Task SaveDocumentAsync(Document doc);
        Task DeleteDocumentAsync(Document doc);

        Task RemovePhotosAsync(IEnumerable<Photo> photos);

    }
}
