using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logic = MyDocs.Common.Model.Logic;

namespace JsonNetDal
{
    public class Document
    {
        public Guid Id { get; set; }

        public string Category { get; set; }

        public DateTime DateAdded { get; set; }

        public TimeSpan Lifespan { get; set; }

        public bool HasLimitedLifespan { get; set; }

        public List<string> Tags { get; set; }

        public List<SubDocument> SubDocuments { get; set; }

        public Document() { }

        public Document(Guid id, string category, DateTime dateAdded, TimeSpan lifespan, bool hasLimitedLifespan, IEnumerable<string> tags, IEnumerable<SubDocument> subDocuments)
        {
            Id = id;
            Category = category;
            Tags = tags.ToList();
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
            SubDocuments = subDocuments.ToList();
        }

        public static Document FromLogic(Logic.Document document)
        {
            return new Document(document.Id, document.Category, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags, document.SubDocuments.Select(SubDocument.FromLogic));
        }

        public async Task<Logic.Document> ToLogic()
        {
            var subDocuments = await Task.WhenAll(SubDocuments.Select(sd => sd.ToLogic()));
            return new Logic.Document(Id, Category, DateAdded, Lifespan, HasLimitedLifespan, Tags, subDocuments);
        }
    }
}
