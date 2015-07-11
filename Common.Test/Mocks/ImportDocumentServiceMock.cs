using MyDocs.Common.Contract.Service;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Common.Test.Mocks
{
    class ImportDocumentServiceMock : IImportDocumentService
    {
        public Func<StorageFile, Task> ImportDocumentsFunc = async delegate { await Task.Yield(); };
        public Task ImportDocuments(StorageFile file)
        {
            return ImportDocumentsFunc(file);
        }
    }
}
