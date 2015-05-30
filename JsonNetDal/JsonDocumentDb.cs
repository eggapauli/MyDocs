﻿using MyDocs.Common.Contract.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Logic = MyDocs.Common.Model.Logic;

namespace JsonNetDal
{
    public class JsonDocumentDb : IDocumentDb
    {
        private readonly IFileConverter fileConverter;
        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            Culture = CultureInfo.InvariantCulture
        };

        public JsonDocumentDb(IFileConverter fileConverter)
        {
            this.fileConverter = fileConverter;
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
        }

        public async Task<IEnumerable<Logic.Document>> GetAllDocumentsAsync()
        {
            var dbDocs = await ReadDocuments();
            return await Task.WhenAll(dbDocs.Select(d => d.ToLogic(fileConverter)));
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
            return await doc.ToLogic(fileConverter);
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
            using (var stream = await dbFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var outputStream = stream.GetOutputStreamAt(0))
            using (var writer = new DataWriter(outputStream))
            {
                writer.WriteString(content);
                //await writer.StoreAsync();
                //await writer.FlushAsync();
            }
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
            using (var stream = await dbFile.OpenAsync(FileAccessMode.Read))
            using (var inputStream = stream.GetInputStreamAt(0))
            using (var reader = new DataReader(inputStream))
            {
                return reader.ReadString((uint)stream.Size);
            }
        }

        private async Task<IStorageFile> GetDbFile()
        {
            return await ApplicationData.Current.LocalFolder.CreateFileAsync("db.json", CreationCollisionOption.OpenIfExists);
        }
    }
}