using MyDocs.Common.Collection;
using MyDocs.Common.Comparer;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class DocumentService : IDocumentService
    {
        private ISettingsService settingsService;
        private IApplicationDataContainer docsDataContainer;

        private readonly IFolder tempFolder = new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder);

        public SortedObservableCollection<Category> Categories { get; private set; }

        public DocumentService(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            Categories = new SortedObservableCollection<Category>(new CategoryComparer());

            docsDataContainer = settingsService.SettingsContainer.CreateContainer("docs");

        }

        private async Task ClearAllData()
        {
            docsDataContainer.Values.Clear();

            var folders = new[] {
                ApplicationData.Current.LocalFolder,
                ApplicationData.Current.RoamingFolder,
                ApplicationData.Current.TemporaryFolder
            };

            foreach (var folder in folders) {
                var files = await folder.GetFilesAsync();
                foreach (var file in files) {
                    await file.DeleteAsync();
                }
            }
        }

        private async Task InsertTestData()
        {
            var service = new MyDocs.WindowsStore.Service.Design.DesignDocumentService();
            await service.LoadCategoriesAsync();
            var documents = service.Categories.SelectMany(c => c.Documents);

            foreach (var document in documents) {
                await SaveDocumentAsync(document);
            }
        }

        public async Task LoadCategoriesAsync()
        {
            if (Categories.Count > 0) {
                return;
            }
#if DEBUG
            await ClearAllData();
            await InsertTestData();
#endif
            //await Task.Delay(2000);
            //categories.Clear();

            var data = from obj in docsDataContainer.Values.Values
                       let item = obj as ApplicationDataCompositeValue
                       let categoryName = (string)(item["Category"])
                       where item != null
                       orderby categoryName
                       group item by categoryName into category
                       select category;

            var i = 0;
            foreach (var group in data) {
                var documents = new List<Document>();
                foreach (ApplicationDataCompositeValue item in group) {
                    documents.Add(await item.ConvertToDocumentAsync());
                    if (++i % 5 == 0) {
                        documents.Add(new AdDocument());
                    }
                }
                Categories.Add(new Category(group.Key, documents));
            }

            await RemoveOutdatedDocuments(Categories);
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return from obj in docsDataContainer.Values.Values
                   let item = obj as ApplicationDataCompositeValue
                   let categoryName = (string)(item["Category"])
                   group item by categoryName into category
                   orderby category.Key
                   select category.Key;
        }

        public Category GetCategoryByName(string name)
        {
            var cat = Categories.FirstOrDefault(c => c.Name == name);
            if (cat == null) {
                cat = new Category(name);
                Categories.Add(cat);
            }
            return cat;
        }

        public async Task RenameCategoryAsync(Category category, string newName)
        {
            if (category.Name == newName) {
                return;
            }

            foreach (var doc in category.Documents) {
                doc.Category = newName;
            }

            var existingCat = Categories.FirstOrDefault(c => c.Name == newName);
            if (existingCat != null) {
                foreach (var doc in category.Documents) {
                    existingCat.Documents.Add(doc);
                }
                Categories.Remove(category);
            }
            else {
                category.Name = newName;
                // Re-sort
                Categories.Remove(category);
                Categories.Add(category);
            }

            foreach (var document in category.Documents) {
                await SaveDocumentAsync(document);
            }
        }

        public async Task<Document> GetDocumentById(Guid id)
        {
            await LoadCategoriesAsync();
            return Categories.SelectMany(c => c.Documents).FirstOrDefault(d => d.Id == id);
        }

        public void DetachDocument(Document doc)
        {
            var docRef = Categories.SelectMany(c => c.Documents).FirstOrDefault(d => d.Id == doc.Id);
            var cat = Categories.FirstOrDefault(c => c.Documents.Contains(docRef));
            if (cat != null) {
                cat.Documents.Remove(docRef);
                if (!cat.Documents.Any(d => !(d is AdDocument))) {
                    Categories.Remove(cat);
                }
            }
        }

        public async Task SaveDocumentAsync(Document doc)
        {
            if (!(doc is AdDocument)) {
                foreach (var file in doc.Photos.SelectMany(p => new[] { p.File, p.Preview })) {
                    string name = Path.GetRandomFileName() + Path.GetExtension(file.Path);

                    if (file.IsInFolder(tempFolder)) {
                        await file.MoveAsync(settingsService.PhotoFolder, name);
                    }
                }
                docsDataContainer.Values[doc.Id.ToString()] = doc.ConvertToStoredDocument();
            }
        }

        public async Task DeleteDocumentAsync(Document doc)
        {
            if (!(doc is AdDocument)) {
                foreach (var photo in doc.Photos) {
                    await photo.File.MoveAsync(tempFolder);
                }

                docsDataContainer.Values.Remove(doc.Id.ToString());

                DetachDocument(doc);
            }
        }

        public async Task RemovePhotosAsync(IEnumerable<IFile> photos)
        {
            foreach (var photo in photos) {
                await photo.DeleteAsync();
            }
        }

        private async Task RemoveOutdatedDocuments(ICollection<Category> categories)
        {
            var docs = (from cat in categories
                        from doc in cat.Documents
                        where !(doc is AdDocument)
                        where doc.HasLimitedLifespan
                        where doc.DateRemoved < DateTime.Today
                        select doc).ToList();

            foreach (var doc in docs) {
                DetachDocument(doc);
                await DeleteDocumentAsync(doc);
            }
        }
    }
}
