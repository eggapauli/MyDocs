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
        public async Task Setup(IEnumerable<Logic.Document> documents)
        {
            using (var db = OpenDatabase()) {
                await db.PurgeAsync();
                db.Save(documents.Select(Document.FromLogic));
            }
        }

        public Task RemoveAllDocumentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Logic.Document>> GetAllDocumentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Logic.Document>> GetDocuments(string categoryName)
        {
            throw new NotImplementedException();
        }

        public Task<Logic.Document> GetDocument(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDistinctCategories()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> GetDistinctDocumentYears()
        {
            throw new NotImplementedException();
        }

        public void Save(Logic.Document document)
        {
            throw new NotImplementedException();
        }

        public void Remove(string documentId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveDocument(Logic.Document document)
        {
            throw new NotImplementedException();
        }

        public Task RemovePhotos(IEnumerable<Logic.Photo> photos)
        {
            throw new NotImplementedException();
        }

        private DbInstance OpenDatabase()
        {
            var db = new DbInstance("mydocs.lex.db");
            db.Map<Document>()
                .Automap(x => x.Id, true)
                .WithIndex("Category", x => x.Category)
                .WithIndex("Tags", x => x.Tags);
            db.Initialize();
            //db.Purge();
            //db.Save(documents);
            return db;
        }
    }
}
