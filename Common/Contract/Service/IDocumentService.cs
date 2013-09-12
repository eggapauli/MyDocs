using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IDocumentService
    {
        ObservableCollection<Document> Documents { get; }
        Task LoadDocumentsAsync();
        IEnumerable<string> GetCategoryNames();
        Task RenameCategoryAsync(string oldName, string newName);

        Task<Document> GetDocumentById(Guid id);
        Task SaveDocumentAsync(Document doc);
        Task DeleteDocumentAsync(Document doc);

        Task RemovePhotosAsync(IEnumerable<IFile> photos);
    }
}
