using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace MyDocs.Common.Model.View
{
#pragma warning disable 0659 // GetHashCode not needed, because documents are not stored in dictionaries

    [DebuggerDisplay("{Id} - {Category}")]
    public class Document : ObservableObject, IEquatable<Document>
    {
        private Guid id;
        private string category;
        private IImmutableList<SubDocument> subDocuments;
        private IImmutableList<string> tags;
        private DateTime dateAdded;
        private TimeSpan lifespan;
        private bool hasLimitedLifespan;

        public Guid Id
        {
            get { return id; }
            private set { Set(ref id, value); }
        }

        public string Category
        {
            get { return category; }
            set { Set(ref category, value); }
        }

        public IImmutableList<SubDocument> SubDocuments
        {
            get { return subDocuments; }
            private set { Set(ref subDocuments, value); }
        }

        public Photo TitlePhoto
        {
            get
            {
                var subDocument = SubDocuments.FirstOrDefault();
                if (subDocument != null) {
                    return subDocument.Photos.FirstOrDefault();
                }
                return null;
            }
        }

        public IImmutableList<string> Tags
        {
            get { return tags; }
            set
            {
                if (Set(ref tags, value)) {
                    RaisePropertyChanged(() => TagsString);
                }
            }
        }

        public DateTime DateAdded
        {
            get { return dateAdded; }
            set { Set(ref dateAdded, value); }
        }

        public TimeSpan Lifespan
        {
            get { return lifespan; }
            set
            {
                if (Set(ref lifespan, value)) {
                    RaisePropertyChanged(() => DateRemoved);
                    RaisePropertyChanged(() => DaysToRemoval);
                }
            }
        }

        public bool HasLimitedLifespan
        {
            get { return hasLimitedLifespan; }
            set { Set(ref hasLimitedLifespan, value); }
        }

        public string TagsString
        {
            get { return string.Join(", ", Tags); }
            set
            {
                var tags = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Tags = tags.Select(tag => tag.Trim()).ToImmutableList();
            }
        }

        public DateTime DateRemoved
        {
            get { return DateAdded.Add(Lifespan).Date; }
            set { Lifespan = value.Subtract(DateAdded); }
        }

        public int DaysToRemoval
        {
            get { return Convert.ToInt32(DateRemoved.Subtract(DateTime.Today).TotalDays); }
        }

        public Document()
            : this(
                Guid.NewGuid(),
                null,
                DateTime.Today.AddDays(1),
                DateTime.Today.AddYears(2).Subtract(DateTime.Today),
                true,
                Enumerable.Empty<string>())
        { }

        public Document(Guid id, string category, DateTime dateAdded, TimeSpan lifespan, bool hasLimitedLifespan, IEnumerable<string> tags)
            : this(id, category, dateAdded, lifespan, hasLimitedLifespan, tags, Enumerable.Empty<SubDocument>())
        { }

        public Document(
            Guid id,
            string category,
            DateTime dateAdded,
            TimeSpan lifespan,
            bool hasLimitedLifespan,
            IEnumerable<string> tags,
            IEnumerable<SubDocument> subDocuments)
        {
            Id = id;
            Category = category;
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
            Tags = tags.ToImmutableList();
            SubDocuments = subDocuments.ToImmutableList();
        }

        public void AddSubDocument(SubDocument doc)
        {
            SubDocuments = SubDocuments.Add(doc);
        }

        public void RemovePhoto(Photo photo)
        {
            var subDocument = SubDocuments.Single(doc => doc.Photos.Contains(photo));
            subDocument.RemovePhoto(photo);
            if (subDocument.Photos.Count == 0) {
                SubDocuments = SubDocuments.Remove(subDocument);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Document);
        }

        public bool Equals(Document other)
        {
            if (other == null) {
                return false;
            }
            return Id.Equals(other.Id);
        }

        public Document Clone()
        {
            return new Document(id, category, dateAdded, lifespan, hasLimitedLifespan, tags, subDocuments);
        }

        public Logic.Document ToLogic()
        {
            return new Logic.Document(Id, Category, DateAdded, Lifespan, HasLimitedLifespan, Tags, SubDocuments.Select(sd => sd.ToLogic()));
        }

        public static Document FromLogic(Logic.Document document)
        {
            return new Document(document.Id, document.Category, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags, document.SubDocuments.Select(SubDocument.FromLogic));
        }
    }
}
