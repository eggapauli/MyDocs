using MyDocs.Common.Contract.Service;
using System;
using System.Threading.Tasks;

namespace Common.Test.Mocks
{
    class ImportDocumentServiceMock : IImportDocumentService
    {
        public Func<Task> ImportDocumentsFunc = async delegate { await Task.Yield(); };
        public Task ImportDocuments()
        {
            return ImportDocumentsFunc();
        }
    }
}
