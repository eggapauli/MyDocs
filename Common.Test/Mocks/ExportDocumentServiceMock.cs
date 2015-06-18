using MyDocs.Common.Contract.Service;
using System;
using System.Threading.Tasks;

namespace Common.Test.Mocks
{
    class ExportDocumentServiceMock : IExportDocumentService
    {
        public Func<Task> ExportDocumentsFunc = async delegate { await Task.Yield(); };
        public Task ExportDocuments()
        {
            return ExportDocumentsFunc();
        }
    }
}
