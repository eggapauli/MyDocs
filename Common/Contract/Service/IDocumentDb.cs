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
        Task ClearAllData();

        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<IEnumerable<Document>> GetDocuments(string categoryName);
        Task<Document> GetDocument(Guid id);

        Task<IEnumerable<string>> GetDistinctCategories();

        Task<IEnumerable<int>> GetDistinctDocumentYears();

        Task Save(Document document);

        Task Remove(Guid documentId);


    }
}
