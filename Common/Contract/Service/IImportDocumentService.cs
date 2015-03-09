using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IImportDocumentService
    {
        Task ImportDocuments();
    }

    public class ImportManifestNotFoundException : Exception
    {
        public ImportManifestNotFoundException() { }
        public ImportManifestNotFoundException(string message) : base(message) { }
        public ImportManifestNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
