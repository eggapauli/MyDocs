using MyDocs.Common;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

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

        public async Task<Document> StoreSubDocumentsPermanent(Document document)
        {
            var tempFolder = await GetTemporaryDocumentFolder(document.Id);
            var permanentFolder = await GetPermanentDocumentFolder(document.Id);
            await tempFolder.MoveRecursive(permanentFolder);
            
            var query = permanentFolder.CreateFileQueryWithOptions(new QueryOptions { FolderDepth = FolderDepth.Deep });
            var files = await query.GetFilesAsync();
            var subDocuments = document.SubDocuments.Select(sd =>
            {
                var file = files.Single(f => f.Name == sd.File.Name);
                var photos = files
                    .Where(f => sd.Photos.Select(p => p.File.Name).Contains(f.Name))
                    .Select(f => new Photo(f));
                return new SubDocument(sd.Title, file, photos);
            });
            return new Document(document.Id, document.Category, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags, subDocuments);
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
