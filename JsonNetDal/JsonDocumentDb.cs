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

        public async Task Setup(IEnumerable<Logic.Document> documents)
        {
            await ClearAllData();

            await WriteDocuments(documents.Select(Document.FromLogic));
            // TODO save photos
        }

        public async Task ClearAllData()
        {
            var dbFile = await GetDbFile();
            await dbFile.DeleteAsync();
            // TODO remove photos

            changed.OnNext(Unit.Default);
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
            var dbDoc = Document.FromLogic(document);
            await WriteDocuments(docs.Concat(Enumerable.Repeat(dbDoc, 1)));
            // TODO save photos
        }

        public async Task Remove(Guid documentId)
        {
            var docs = await ReadDocuments();
            await WriteDocuments(docs.Where(d => d.Id != documentId));
            // TODO remove photos
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
