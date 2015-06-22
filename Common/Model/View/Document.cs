using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace MyDocs.Common.Model.View
{
#pragma warning disable 0659 // GetHashCode not needed, because documents are not stored in dictionaries

    [DebuggerDisplay("{Id} - {Category}")]
    public class Document : ReactiveObject, IEquatable<Document>
    {
        private readonly Guid id;
        private string category;
        private IImmutableList<SubDocument> subDocuments;
        private IImmutableList<string> tags;
        private DateTime dateAdded;
        private bool hasLimitedLifespan;

        public Guid Id
        {
            get { return id; }
        }

        public string Category
        {
            get { return category; }
            set { this.RaiseAndSetIfChanged(ref category, value); }
        }

        public IImmutableList<SubDocument> SubDocuments
        {
            get { return subDocuments; }
            internal set { this.RaiseAndSetIfChanged(ref subDocuments, value); }
        }

        private readonly ObservableAsPropertyHelper<Photo> titlePhoto;
        public Photo TitlePhoto
        {
            get { return titlePhoto.Value; }
        }
        
        public IImmutableList<string> Tags
        {
            get { return tags; }
            set { this.RaiseAndSetIfChanged(ref tags, value); }
        }

        public DateTime DateAdded
        {
            get { return dateAdded; }
            set { this.RaiseAndSetIfChanged(ref dateAdded, value); }
        }

        private readonly ObservableAsPropertyHelper<TimeSpan> lifespan;
        public TimeSpan Lifespan
        {
            get { return lifespan.Value; }
        }

        public bool HasLimitedLifespan
        {
            get { return hasLimitedLifespan; }
            set { this.RaiseAndSetIfChanged(ref hasLimitedLifespan, value); }
        }
        
        private DateTime dateRemoved;
        public DateTime DateRemoved
        {
            get { return dateRemoved; }
            set { this.RaiseAndSetIfChanged(ref dateRemoved, value); }
        }

        private readonly ObservableAsPropertyHelper<int> daysToRemoval;
        public int DaysToRemoval
        {
            get { return daysToRemoval.Value; }
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
            this.id = id;
            Category = category;
            DateAdded = dateAdded;
            DateRemoved = DateAdded.Add(lifespan);
            HasLimitedLifespan = hasLimitedLifespan;
            Tags = tags.ToImmutableList();
            SubDocuments = subDocuments.ToImmutableList();

            titlePhoto = this.WhenAnyValue(x => x.SubDocuments)
                .Select(x => SubDocuments.FirstOrDefault()?.Photos.FirstOrDefault())
                .ToProperty(this, x => x.TitlePhoto);

            this.lifespan = this.WhenAnyValue(x => x.DateAdded, x => x.DateRemoved, (added, removed) => removed.Subtract(added))
                .ToProperty(this, x => x.Lifespan);

            daysToRemoval = this.WhenAnyValue(x => x.DateRemoved)
                .Select(x => (int)(x.Subtract(DateTime.Today).TotalDays))
                .ToProperty(this, x => x.DaysToRemoval);
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
            return new Document(Id, Category, DateAdded, Lifespan, HasLimitedLifespan, Tags, SubDocuments);
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
