using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IDocumentDb
    {
        IObservable<Unit> Changed { get; }

        Task ClearAllData();

        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<IEnumerable<Document>> GetDocuments(string categoryName);
        Task<Document> GetDocument(Guid id);

        Task<IEnumerable<string>> GetDistinctCategories();

        Task<IEnumerable<int>> GetDistinctDocumentYears();

        Task Save(Document document);

        Task Delete(Guid documentId);
        Task Delete(IEnumerable<Guid> documentIds);
    }


    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException() { }
        public DocumentNotFoundException(string message) : base(message) { }
        public DocumentNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
