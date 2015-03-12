using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace MyDocs.Common.Model
{
    #pragma warning disable 0659 // GetHashCode not needed, because documents are not stored in dictionaries

    [DebuggerDisplay("{Id} - {Category}")]
    public class Document : ObservableObject
    {
        private Guid id;
        private string category;
        private ObservableCollection<Photo> photos;
        private ObservableCollection<string> tags;
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

        public ObservableCollection<Photo> Photos
        {
            get { return photos; }
            private set { Set(ref photos, value); }
        }

        public IEnumerable<Photo> Previews
        {
            get
            {
                foreach (var photo in photos) {
                    if (photo.Previews.Any()) {
                        foreach (var preview in photo.Previews) {
                            yield return new Photo(photo.Title, preview);
                        }
                    }
                    else {
                        yield return new Photo(photo.Title, photo.File);
                    }
                }
            }
        }

        public Photo TitlePhoto
        {
            get { return Photos.FirstOrDefault(); }
        }

        public ObservableCollection<string> Tags
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
            get { return String.Join(", ", Tags); }
            set
            {
                string[] tags = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Tags = new ObservableCollection<string>(tags.Select(tag => tag.Trim()));
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
        {
            Id = Guid.NewGuid();
            DateAdded = DateTime.Today.AddDays(1);
            Lifespan = DateTime.Today.AddYears(2).Subtract(DateTime.Today);
            HasLimitedLifespan = true;
            Tags = new ObservableCollection<string>();
            Photos = new ObservableCollection<Photo>();
        }

        public Document(
            Guid id,
            string category,
            DateTime dateAdded,
            TimeSpan lifespan,
            bool hasLimitedLifespan,
            IEnumerable<string> tags,
            IEnumerable<Photo> photos = null)
        {
            Id = id;
            Category = category;
            DateAdded = dateAdded;
            Lifespan = lifespan;
            HasLimitedLifespan = hasLimitedLifespan;
            Tags = tags as ObservableCollection<string> ?? new ObservableCollection<string>(tags);

            if (photos != null) {
                Photos = photos as ObservableCollection<Photo> ?? new ObservableCollection<Photo>(photos);
            }
            else {
                Photos = new ObservableCollection<Photo>();
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Document;
            if (other == null) {
                return false;
            }

            return this.Id.Equals(other.Id);
        }

        public Document Clone()
        {
            return new Document(id, category, dateAdded, lifespan, hasLimitedLifespan, tags, new ObservableCollection<Photo>(photos));
        }
    }
}
