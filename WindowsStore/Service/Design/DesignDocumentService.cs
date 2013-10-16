using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service.Design
{
    public class DesignDocumentService : IDocumentService
    {
        private Random random = new Random();
        
        public ObservableCollection<Document> Documents { get; set; }

        public DesignDocumentService()
        {
            Documents = new ObservableCollection<Document>();
        }

        public async Task LoadDocumentsAsync()
        {
            IList<IFile> photos;
            try {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("design");
                photos = (await folder.GetFilesAsync()).Select<StorageFile, IFile>(f => new WindowsStoreFile(f)).ToList();
            }
            catch (Exception) {
                photos = null;
            }
            foreach (var document in CreateDocuments(photos)) {
                Documents.Add(document);
            }
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return Enumerable.Range(1, 5).Select(i => "Category " + i);
        }

        private IEnumerable<Document> CreateDocuments(IList<IFile> photos)
        {
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < i + 2; j++) {
                    yield return new Document(
                        Guid.NewGuid(),
                        "Category " + (i + 1),
                        DateTime.Today,
                        TimeSpan.FromDays(30),
                        true,
                        Enumerable.Range(i, i + 1 + 2 * j).Select(idx => "Tag" + (idx + 1)),
                        GetRandomPhotos(photos));
                }
            }
        }

        private IEnumerable<Photo> GetRandomPhotos(IList<IFile> photos)
        {
            if (photos == null) {
                yield break;
            }
            int count = random.Next(1, 4);
            while (count > 0 && photos.Count > 0) {
                int idx = random.Next(photos.Count);
                yield return new Photo(photos[idx].Name, photos[idx]);
                photos.RemoveAt(idx);
                count--;
            }
        }

        public Task RenameCategoryAsync(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<Document> GetDocumentById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task SaveDocumentAsync(Document doc)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDocumentAsync(Document doc)
        {
            throw new NotImplementedException();
        }

        public Task RemovePhotosAsync(IEnumerable<Photo> photos)
        {
            throw new NotImplementedException();
        }
    }
}
