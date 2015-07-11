using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface IImportDocumentService
    {
        Task ImportDocuments(StorageFile file);
    }

    public class ImportManifestNotFoundException : Exception
    {
        public ImportManifestNotFoundException() { }
        public ImportManifestNotFoundException(string message) : base(message) { }
        public ImportManifestNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
