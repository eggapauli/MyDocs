using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Model.Serializable
{
    [DataContract(Namespace = "http://mydocs.eggapauli")]
    public class Document
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public IEnumerable<string> Tags { get; set; }

        [DataMember]
        public DateTime DateAdded { get; set; }

        [DataMember]
        public TimeSpan Lifespan { get; set; }

        [DataMember]
        public bool HasLimitedLifespan { get; set; }

        [DataMember]
        public IEnumerable<string> Files { get; set; }

        public Document(Guid id, string category, IEnumerable<string> tags, DateTime dateAdded, TimeSpan lifespan, bool hasLimitedLifespan, IEnumerable<string> files)
        {
            Id = id;
            Category = category;
            Tags = tags.ToList();
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
            Files = files.ToList();
        }

        public static Document FromModel(Logic.Document d)
        {
            var files = d.SubDocuments.Select(p => p.File);
            return new Serializable.Document(d.Id, d.Category, d.Tags, d.DateAdded, d.Lifespan, d.HasLimitedLifespan, files.Select(f => f.GetRelativePath()));
        }
    }
}
