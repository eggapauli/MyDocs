using MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IPersistDocuments
    {
        Task RemoveAllDocumentsAsync();

        Task<IEnumerable<Document>> GetAllDocumentsAsync();

        IEnumerable<string> GetDistinctCategories();

        IEnumerable<int> GetDistinctDocumentYears();

        Task SaveAsync(Document document);

        void Remove(string documentId);

        Task RemovePhotosAsync(IEnumerable<Photo> photos);
    }
}
