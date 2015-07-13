using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Common.Test.Mocks
{
    class SubDocumentServiceMock : ISubDocumentService
    {
        public Task DeleteSubDocuments(Guid documentId)
        {
            throw new NotImplementedException();
        }

        public Task<StorageFile> StoreCameraFileForDocument(StorageFile file, Guid documentId)
        {
            throw new NotImplementedException();
        }

        public Task<Document> StoreSubDocumentsPermanent(Document document)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StorageFile>> StoreUserFilesForDocument(IEnumerable<StorageFile> files, Guid documentId)
        {
            throw new NotImplementedException();
        }
    }
}
