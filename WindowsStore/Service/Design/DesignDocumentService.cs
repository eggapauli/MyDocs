using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service.Design
{
    public class DesignDocumentService : IDocumentService
    {
        private readonly Random random = new Random();

        public IObservable<IEnumerable<Document>> GetDocuments()
        {
            return Observable.FromAsync(async () =>
            {
                var photos = new List<StorageFile>();
                try
                {
                    var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("design");
                    photos.AddRange(await folder.GetFilesAsync());
                }
                catch { }
                return CreateDocuments(photos);
            });
        }

        public IObservable<IEnumerable<string>> GetCategoryNames()
        {
            return Observable.Return(Enumerable.Range(1, 5).Select(i => "Category " + i));
        }

        public IObservable<IEnumerable<int>> GetDistinctDocumentYears()
        {
            return Observable.Return(new[] { DateTime.Today.Year });
        }

        private IEnumerable<Document> CreateDocuments(IList<StorageFile> photos)
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
                        GetRandomSubDocuments(photos));
                }
            }
        }

        private IEnumerable<SubDocument> GetRandomSubDocuments(IList<StorageFile> photos)
        {
            if (photos == null) {
                yield break;
            }
            int count = random.Next(1, 4);
            while (count > 0 && photos.Count > 0) {
                int idx = random.Next(photos.Count);
                yield return new SubDocument(photos[idx], new[] { new Photo (photos[idx]) });
                photos.RemoveAt(idx);
                count--;
            }
        }

        public Task DeleteCategoryAsync(string categoryName)
        {
            throw new NotImplementedException();
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

        public Task RemoveOutdatedDocuments()
        {
            throw new NotImplementedException();
        }

        public event EventHandler Changed;
    }
}
