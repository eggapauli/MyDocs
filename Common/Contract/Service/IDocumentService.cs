using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IDocumentService
    {
        IObservable<IEnumerable<Document>> GetDocuments();
        IObservable<IEnumerable<string>> GetCategoryNames();
        IObservable<IEnumerable<int>> GetDistinctDocumentYears();

        Task RenameCategoryAsync(string oldName, string newName);
        Task DeleteCategoryAsync(string categoryName);

        Task<Document> GetDocumentById(Guid id);
        Task SaveDocumentAsync(Document doc);
        Task DeleteDocumentAsync(Document doc);

        Task RemovePhotosAsync(IEnumerable<Photo> photos);

        Task RemoveOutdatedDocuments();
    }
}
