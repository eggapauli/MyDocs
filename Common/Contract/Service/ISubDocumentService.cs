using MyDocs.Common.Model.Logic;
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
        Task<Document> StoreSubDocumentsPermanent(Document document);
        Task DeleteSubDocuments(Guid documentId);
    }
}
