using Lex.Db;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using Logic = MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDbDal
{
    public class LexDocumentDb : IDocumentDb
    {
        private readonly Func<DbInstance> dbFactory;
        private readonly IFileConverter fileConverter;

        public LexDocumentDb(Func<DbInstance> dbFactory, IFileConverter fileConverter)
        {
            this.dbFactory = dbFactory;
            this.fileConverter = fileConverter;
        }

        public async Task Setup(IEnumerable<Logic.Document> documents)
        {
            using (var db = OpenDatabase()) {
                await db.PurgeAsync();
                // TODO use transaction
                db.Save(documents.Select(Document.FromLogic));
                db.Save(documents.Select(SubDocument.FromLogic));
            }
        }

        public async Task ClearAllData()
        {
            using (var db = OpenDatabase()) {
                await db.PurgeAsync();
            }
        }

        public async Task<IEnumerable<Logic.Document>> GetAllDocumentsAsync()
        {
            using (var db = OpenDatabase()) {
                var documents = await db.Table<Document>().LoadAllAsync();
                var subDocuments = await db.Table<SubDocument>().LoadAllAsync();
                return await ConvertDocuments(documents, subDocuments);
            }
        }

        public async Task<IEnumerable<Logic.Document>> GetDocuments(string categoryName)
        {
            using (var db = OpenDatabase()) {
                var documents = await db.Table<Document>()
                    .IndexQuery<string>("Category")
                    .Key(categoryName)
                    .ToListAsync();
                return await ConvertDocuments(documents, db);
            }
        }

        public async Task<Logic.Document> GetDocument(Guid id)
        {
            using (var db = OpenDatabase()) {
                var document = await db.Table<Document>().LoadByKeyAsync(id);
                return document != null ? await ConvertDocument(document, db) : null;
            }
        }

        public async Task<IEnumerable<string>> GetDistinctCategories()
        {
            using (var db = OpenDatabase()) {
                // TODO this could be slow
                var document = await db.Table<Document>().LoadAllAsync();
                return document.Select(d => d.Category).Distinct();
            }
        }

        public async Task<IEnumerable<int>> GetDistinctDocumentYears()
        {
            using (var db = OpenDatabase()) {
                // TODO this could be slow
                var document = await db.Table<Document>().LoadAllAsync();
                return document
                    .Select(d => d.DateAdded.Year)
                    .Distinct()
                    .OrderBy(x => x);
            }
        }

        public async Task Save(Logic.Document document)
        {
            using (var db = OpenDatabase()) {
                // TODO use transaction
                await db.Table<Document>().SaveAsync(Document.FromLogic(document));
                await db.Table<SubDocument>().SaveAsync(SubDocument.FromLogic(document));
            }
        }

        public async Task Remove(Guid documentId)
        {
            using (var db = OpenDatabase()) {
                // TODO use transaction
                db.Table<Document>().DeleteByKey(documentId);
                var subDocuments = await GetSubDocuments(new[] { documentId }, db);
                await db.Table<SubDocument>().DeleteAsync(subDocuments);
            }
        }

        public async Task RemoveDocument(Logic.Document document)
        {
            using (var db = OpenDatabase()) {
                await db.Table<Document>().DeleteByKeyAsync(document.Id);
            }
        }

        public async Task RemovePhotos(IEnumerable<Logic.Photo> photos)
        {
            await Task.Yield();
            //throw new NotImplementedException();
        }

        private DbInstance OpenDatabase()
        {
            var db = dbFactory();
            db.Map<Document>()
                .Automap(x => x.Id, true)
                .WithIndex("Category", x => x.Category);
                //.WithIndex("Tags", x => x.Tags);
            db.Map<SubDocument>()
                .Automap(x => x.Id, true)
                .WithIndex("FK", x => x.DocumentId);
            db.Initialize();
            return db;
        }

        private async Task<Logic.Document> ConvertDocument(Document document, DbInstance db)
        {
            return (await ConvertDocuments(new [] { document }, db)).Single();
        }

        private async Task<IEnumerable<Logic.Document>> ConvertDocuments(IEnumerable<Document> documents, DbInstance db)
        {
            var subDocuments = await GetSubDocuments(documents, db);
            return await ConvertDocuments(documents, subDocuments);
        }

        private static Task<List<SubDocument>> GetSubDocuments(IEnumerable<Document> documents, DbInstance db)
        {
            return GetSubDocuments(documents.Select(d => d.Id), db);
        }

        private static async Task<List<SubDocument>> GetSubDocuments(IEnumerable<Guid> documentIds, DbInstance db)
        {
            return await db.Table<SubDocument>()
                .IndexQuery<Guid>("FK")
                .Where(id => documentIds.Contains(id))
                .ToListAsync();
        }

        private async Task<IEnumerable<Logic.Document>> ConvertDocuments(IEnumerable<Document> documents, IEnumerable<SubDocument> allSubDocuments)
        {
            var convertedSubDocumentsTask = allSubDocuments.Select(sd => sd.ToLogic(fileConverter));
            var convertedSubDocuments = await Task.WhenAll(convertedSubDocumentsTask);
            return documents.Select(d => {
                var subDocuments = convertedSubDocuments
                    .Where(sd => sd.Item1 == d.Id)
                    .Select(sd => sd.Item2);
                return d.ToLogic(subDocuments);
            });
        }
    }
}
