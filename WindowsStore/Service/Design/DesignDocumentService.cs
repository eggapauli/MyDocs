﻿using MyDocs.Common.Collection;
using MyDocs.Common.Comparer;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service.Design
{
    public class DesignDocumentService : IDocumentService
    {
        private SortedObservableCollection<Category> categories;

        public SortedObservableCollection<Category> Categories
        {
            get { return categories; }
        }

        public DesignDocumentService()
        {
            categories = new SortedObservableCollection<Category>(new CategoryComparer());
        }

        public async Task LoadCategoriesAsync()
        {
            IList<IFile> photos;
            try {
                StorageFolder folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("design");
                photos = (await folder.GetFilesAsync()).Select<StorageFile, IFile>(f => new WindowsStoreFile(f)).ToList();
            }
            catch (Exception) {
                photos = null;
            }
            categories = new SortedObservableCollection<Category>(CreateCategories(photos), new CategoryComparer());
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return Enumerable.Range(1, 5).Select(i => "Category " + i);
        }

        private IEnumerable<Category> CreateCategories(IList<IFile> photos)
        {
            for (int i = 0; i < 2; i++) {
                Category cat = new Category("Category " + (i + 1));
                for (int j = 0; j < i + 2; j++) {
                    Document doc = new Document(
                        Guid.NewGuid(),
                        cat.Name,
                        DateTime.Today,
                        TimeSpan.FromDays(30),
                        true,
                        Enumerable.Range(i, i + 1 + 2 * j).Select(idx => "Tag" + (idx + 1)),
                        GetRandomPhotos(photos)
                    );
                    cat.Documents.Add(doc);
                }
                yield return cat;
            }
        }

        private Random rand = new Random();
        private IEnumerable<Photo> GetRandomPhotos(IList<IFile> photos)
        {
            if (photos == null) {
                yield break;
            }
            int count = rand.Next(1, 4);
            while (count > 0 && photos.Count > 0) {
                int idx = rand.Next(photos.Count);
                yield return new Photo(photos[idx].Name, photos[idx]);
                photos.RemoveAt(idx);
                count--;
            }
        }

        public Task RenameCategoryAsync(Category cat, string NewCategoryName)
        {
            throw new NotImplementedException();
        }

        public Task SaveDocumentAsync(Document document)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDocumentAsync(Document doc)
        {
            throw new NotImplementedException();
        }

        public Task RemovePhotosAsync(IEnumerable<IFile> photos)
        {
            throw new NotImplementedException();
        }

        public Category GetCategoryByName(string name)
        {
            throw new NotImplementedException();
        }

        public void DetachDocument(Document doc)
        {
            throw new NotImplementedException();
        }

        public Task<Document> GetDocumentById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
