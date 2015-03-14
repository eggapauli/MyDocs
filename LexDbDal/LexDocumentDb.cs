using Lex.Db;
using MyDocs.Common.Contract.Service;
using Model = MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexDbDal
{
    public class LexDocumentDb : IDocumentDb
    {
        public async Task Setup(IEnumerable<Model.Document> documents)
        {
            using (var db = OpenDatabase()) {
                await db.PurgeAsync();
                db.Save(documents.Select(Convert));
            }
        }

        public Task RemoveAllDocumentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Model.Document>> GetAllDocumentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Model.Document>> GetDocuments(string categoryName)
        {
            throw new NotImplementedException();
        }

        public Task<Model.Document> GetDocument(Guid id)
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

        public void Save(Model.Document document)
        {
            throw new NotImplementedException();
        }

        public void Remove(string documentId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveDocument(Model.Document document)
        {
            throw new NotImplementedException();
        }

        public Task RemovePhotos(IEnumerable<Model.Photo> photos)
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

        private Document Convert(Model.Document document)
        {
            var subDocuments = document.SubDocuments.Select(sd => new SubDocument(sd.Title, sd.File.GetUri(), sd.Photos.Select(p => p.File.GetUri())));
            return new Document(document.Id, document.Category, subDocuments, document.Tags, document.DateAdded, document.Lifespan, document.HasLimitedLifespan);
        }
    }
}
