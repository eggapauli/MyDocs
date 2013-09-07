using System;
using System.Collections.Generic;
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
            Tags = tags;
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
            Files = files;
        }
    }
}
