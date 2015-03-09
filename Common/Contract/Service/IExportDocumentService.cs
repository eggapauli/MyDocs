using MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IExportDocumentService
    {
        Task ExportDocuments(IReadOnlyCollection<Document> documents);
    }
}
