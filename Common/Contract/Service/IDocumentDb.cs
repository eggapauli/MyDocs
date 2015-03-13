using MyDocs.Common.Model;
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

        Task RemovePhotosAsync(IEnumerable<Photo> photos);


    }
}
