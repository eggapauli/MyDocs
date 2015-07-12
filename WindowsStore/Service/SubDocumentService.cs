using MyDocs.Common;
using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class SubDocumentService : ISubDocumentService
    {
        public async Task<StorageFile> StoreCameraFileForDocument(StorageFile file, Guid documentId)
        {
            var folder = await GetTemporaryDocumentFolder(documentId);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Name);
            await file.MoveAsync(folder, fileName).AsTask();
            return file;
        }

        public async Task<IEnumerable<StorageFile>> StoreUserFilesForDocument(IEnumerable<StorageFile> files, Guid documentId)
        {
            var folder = await GetTemporaryDocumentFolder(documentId);
            var tasks = files.Select(file =>
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Name);
                return file.CopyAsync(folder, fileName).AsTask();
            });
            return await Task.WhenAll(tasks);
        }

        public async Task StoreSubDocumentsPermanent(Guid documentId)
        {
            var tempFolder = await GetTemporaryDocumentFolder(documentId);
            var permanentFolder = await GetPermanentDocumentFolder(documentId);
            await tempFolder.MoveRecursive(permanentFolder);
        }

        public async Task DeleteSubDocuments(Guid documentId)
        {
            var folder = await GetPermanentDocumentFolder(documentId);
            await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        private static async Task<StorageFolder> GetTemporaryDocumentFolder(Guid documentId)
        {
            return await GetDocumentFolder(ApplicationData.Current.TemporaryFolder, documentId);
        }

        private static async Task<StorageFolder> GetPermanentDocumentFolder(Guid documentId)
        {
            return await GetDocumentFolder(ApplicationData.Current.LocalFolder, documentId);
        }

        private static Task<StorageFolder> GetDocumentFolder(IStorageFolder folder, Guid documentId)
        {
            return folder.CreateFolderAsync(documentId.ToString(), CreationCollisionOption.OpenIfExists).AsTask();
        }
    }
}
