using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface ISubDocumentService
    {
        Task<IEnumerable<StorageFile>> StoreUserFilesForDocument(IEnumerable<StorageFile> files, Guid documentId);
        Task<StorageFile> StoreCameraFileForDocument(StorageFile file, Guid documentId);
        Task StoreSubDocumentsPermanent(Guid documentId);
        Task DeleteSubDocuments(Guid documentId);
    }
}
