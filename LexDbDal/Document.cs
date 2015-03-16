using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic = MyDocs.Common.Model.Logic;

namespace LexDbDal
{
    public class Document
    {
        public Guid Id { get; set; }

        public string Category { get; set; }

        public DateTime DateAdded { get; set; }

        public TimeSpan Lifespan { get; set; }

        public bool HasLimitedLifespan { get; set; }

        public List<string> Tags { get; set; }

        public Document() { }

        public Document(Guid id, string category, DateTime dateAdded, TimeSpan lifespan, bool hasLimitedLifespan, IEnumerable<string> tags)
        {
            Id = id;
            Category = category;
            Tags = tags.ToList();
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
        }

        public static Document FromLogic(Logic.Document document)
        {
            return new Document(document.Id, document.Category, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags);
        }

        public Logic.Document ToLogic(IEnumerable<Logic.SubDocument> subDocuments)
        {
            return new Logic.Document(Id, Category, DateAdded, Lifespan, HasLimitedLifespan, Tags, subDocuments);
        }
    }
}
