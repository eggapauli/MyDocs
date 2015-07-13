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
            var folder = await CreateTemporarySubDocumentFolder(documentId);
            await file.MoveAsync(folder, file.Name, NameCollisionOption.GenerateUniqueName);
            return file;
        }

        public async Task<IEnumerable<StorageFile>> StoreUserFilesForDocument(IEnumerable<StorageFile> files, Guid documentId)
        {
            var tasks = files.Select(async file =>
            {
                var folder = await CreateTemporarySubDocumentFolder(documentId);
                return await file.CopyAsync(folder, file.Name, NameCollisionOption.GenerateUniqueName);
            });
            return await Task.WhenAll(tasks);
        }

        public async Task<Document> StoreSubDocumentsPermanent(Document document)
        {
            var sourceBaseFolder = await GetOrCreateDocumentFolder(ApplicationData.Current.TemporaryFolder, document.Id);
            var targetBaseFolder = await GetOrCreateDocumentFolder(ApplicationData.Current.LocalFolder, document.Id);
            var moveTasks = document.SubDocuments.Select(async sd =>
            {
                var sourceFolder = await sd.File.GetParentAsync();
                var folder = await targetBaseFolder.CreateFolderAsync(sourceFolder.Name);
                var tasks = new[] { sd.File }
                    .Concat(sd.Photos.Select(p => p.File))
                    .Distinct(x => x.Path)
                    .Select(f => f.MoveAsync(folder).AsTask());
                await Task.WhenAll(tasks);

                await DeleteIfEmpty(sourceFolder);
            });
            await Task.WhenAll(moveTasks);
            await DeleteIfEmpty(sourceBaseFolder);

            var query = targetBaseFolder.CreateFileQueryWithOptions(new QueryOptions { FolderDepth = FolderDepth.Deep });
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

        private static async Task DeleteIfEmpty(StorageFolder sourceFolder)
        {
            if ((await sourceFolder.GetItemsAsync()).Count == 0)
            {
                await sourceFolder.DeleteAsync();
            }
        }

        public async Task DeleteSubDocuments(Guid documentId)
        {
            var folder = await CreatePermanentSubDocumentFolder(documentId);
            await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        private static Task<StorageFolder> CreateTemporarySubDocumentFolder(Guid documentId)
        {
            return CreateSubDocumentFolder(ApplicationData.Current.TemporaryFolder, documentId);
        }

        private static Task<StorageFolder> CreatePermanentSubDocumentFolder(Guid documentId)
        {
            return CreateSubDocumentFolder(ApplicationData.Current.LocalFolder, documentId);
        }

        private static async Task<StorageFolder> CreateSubDocumentFolder(IStorageFolder folder, Guid documentId)
        {
            var documentFolder = await GetOrCreateDocumentFolder(folder, documentId);
            return await documentFolder.CreateFolderAsync(Guid.NewGuid().ToString());
        }

        private static Task<StorageFolder> GetOrCreateDocumentFolder(IStorageFolder folder, Guid documentId)
        {
            return folder.CreateFolderAsync(documentId.ToString(), CreationCollisionOption.OpenIfExists).AsTask();
        }
    }
}
