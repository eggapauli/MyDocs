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

        public ICollection<SubDocument> SubDocuments { get; set; }

        public ICollection<string> Tags { get; set; }

        public DateTime DateAdded { get; set; }

        public TimeSpan Lifespan { get; set; }

        public bool HasLimitedLifespan { get; set; }

        public Document() { }

        public Document(Guid id, string category, DateTime dateAdded, TimeSpan lifespan, bool hasLimitedLifespan, IEnumerable<string> tags, IEnumerable<SubDocument> subDocuments)
        {
            Id = id;
            Category = category;
            SubDocuments = subDocuments.ToList();
            Tags = tags.ToList();
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
        }

        public static Document FromLogic(Logic.Document document)
        {
            return new Document(document.Id, document.Category, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags, document.SubDocuments.Select(SubDocument.FromLogic));
        }
    }
}
