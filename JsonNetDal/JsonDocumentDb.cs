using MyDocs.Common.Contract.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Logic = MyDocs.Common.Model.Logic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace JsonNetDal
{
    public class JsonDocumentDb : IDocumentDb
    {
        private readonly ISubDocumentService subDocumentService;

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            Culture = CultureInfo.InvariantCulture
        };

        private readonly ISubject<Unit> changed = new Subject<Unit>();
        public IObservable<Unit> Changed
        {
            get { return changed.AsObservable(); }
        }

        public JsonDocumentDb(ISubDocumentService subDocumentService)
        {
            this.subDocumentService = subDocumentService;
        }

        public async Task Setup(IEnumerable<Logic.Document> documents)
        {
            await ClearAllData();

            await WriteDocuments(documents.Select(Document.FromLogic));
            // TODO save photos
        }

        public async Task ClearAllData()
        {
            var dbFile = await GetDbFile();
            await dbFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            await ClearFolder(ApplicationData.Current.LocalFolder);
            changed.OnNext(Unit.Default);
        }

        private async Task ClearFolder(StorageFolder folder)
        {
            var storageItems = await folder.GetItemsAsync();
            var deleteTasks = storageItems
                .Select(f => f.DeleteAsync(StorageDeleteOption.PermanentDelete))
                .Select(x => x.AsTask());
            await Task.WhenAll(deleteTasks);
        }

        public async Task<IEnumerable<Logic.Document>> GetAllDocumentsAsync()
        {
            var dbDocs = await ReadDocuments();
            return await Task.WhenAll(dbDocs.Select(d => d.ToLogic()));
        }

        public async Task<IEnumerable<Logic.Document>> GetDocuments(string categoryName)
        {
            var docs = await GetAllDocumentsAsync();
            return docs.Where(d => d.Category == categoryName);
        }

        public async Task<Logic.Document> GetDocument(Guid id)
        {
            var docs = await ReadDocuments();
            var doc = docs.FirstOrDefault(d => d.Id == id);
            if (doc == null)
            {
                throw new DocumentNotFoundException();
            }
            return await doc.ToLogic();
        }

        public async Task<IEnumerable<string>> GetDistinctCategories()
        {
            var docs = await ReadDocuments();
            return docs.Select(d => d.Category).Distinct();
        }

        public async Task<IEnumerable<int>> GetDistinctDocumentYears()
        {
            var docs = await ReadDocuments();
            return docs
                .Select(d => d.DateAdded.Year)
                .Distinct()
                .OrderBy(x => x);
        }

        public async Task Save(Logic.Document document)
        {
            var docs = await ReadDocuments();
            var doc = await subDocumentService.StoreSubDocumentsPermanent(document);
            var dbDoc = Document.FromLogic(doc);
            await WriteDocuments(docs.Where(d => d.Id != dbDoc.Id).Concat(Enumerable.Repeat(dbDoc, 1)));
        }

        public Task Delete(Guid documentId)
        {
            return Delete(Enumerable.Repeat(documentId, 1));
        }

        public async Task Delete(IEnumerable<Guid> documentIds)
        {
            var docs = await ReadDocuments();
            var remainingDocs = docs.Where(d => !documentIds.Contains(d.Id));
            await WriteDocuments(remainingDocs);
            var deleteSubDocumentsTasks = documentIds.Select(subDocumentService.DeleteSubDocuments);
            await Task.WhenAll(deleteSubDocumentsTasks);
        }

        private async Task RemoveSubDocuments(Document document)
        {
            var removeTasks = document.SubDocuments
                .Select(async sd =>
                {
                    var tasks = sd.Photos
                        .Concat(new[] { sd.File })
                        .Distinct()
                        .Select(RemoveFile);
                    await Task.WhenAll(tasks);
                });
            await Task.WhenAll(removeTasks);
        }

        private async Task RemoveFile(Uri fileUrl)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(fileUrl);
            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        private async Task WriteDocuments(IEnumerable<Document> documents)
        {
            var json = JsonConvert.SerializeObject(documents, serializerSettings);
            await WriteDbFile(json);
        }

        private async Task WriteDbFile(string content)
        {
            var dbFile = await GetDbFile();
            await FileIO.WriteTextAsync(dbFile, content);
            changed.OnNext(Unit.Default);
        }

        private async Task<IEnumerable<Document>> ReadDocuments()
        {
            var json = await ReadDbFile();
            return JsonConvert.DeserializeObject<IEnumerable<Document>>(json, serializerSettings)
                ?? Enumerable.Empty<Document>();
        }

        private async Task<string> ReadDbFile()
        {
            var dbFile = await GetDbFile();
            return await FileIO.ReadTextAsync(dbFile);
        }

        private async Task<IStorageFile> GetDbFile()
        {
            return await ApplicationData.Current.LocalFolder.CreateFileAsync("db.json", CreationCollisionOption.OpenIfExists);
        }
    }
}
