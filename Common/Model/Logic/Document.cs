using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MyDocs.Common.Model.Logic
{
    public class Document
    {
        public Guid Id { get; private set; }

        public string Category { get; private set; }

        public IImmutableList<SubDocument> SubDocuments { get; private set; }

        public IImmutableList<string> Tags { get; private set; }

        public DateTime DateAdded { get; private set; }

        public TimeSpan Lifespan { get; private set; }

        public DateTime DateRemoved
        {
            get { return DateAdded.Add(Lifespan); }
        }

        public bool HasLimitedLifespan { get; private set; }

        public Document(Guid id, string category, DateTime dateAdded, TimeSpan lifespan, bool hasLimitedLifespan, IEnumerable<string> tags)
            :this (id, category, dateAdded, lifespan, hasLimitedLifespan, tags, Enumerable.Empty<SubDocument>())
        { }

        public Document(Guid id, string category, DateTime dateAdded, TimeSpan lifespan, bool hasLimitedLifespan, IEnumerable<string> tags, IEnumerable<SubDocument> subDocuments)
        {
            Id = id;
            Category = category;
            SubDocuments = subDocuments.ToImmutableList();
            Tags = tags.ToImmutableList();
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
        }
    }
}
