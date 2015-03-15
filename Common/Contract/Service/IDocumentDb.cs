using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IDocumentDb
    {
        Task RemoveAllDocumentsAsync();

        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<IEnumerable<Document>> GetDocuments(string categoryName);
        Task<Document> GetDocument(Guid id);

        IEnumerable<string> GetDistinctCategories();

        IEnumerable<int> GetDistinctDocumentYears();

        void Save(Document document);

        void Remove(string documentId);

        Task RemoveDocument(Document document);

        Task RemovePhotos(IEnumerable<Photo> photos);
    }
}
