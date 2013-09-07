using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace MyDocs.Common.Model
{
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
            set { Set(ref id, value); }
        }

        public string Category
        {
            get { return category; }
            set { Set(ref category, value); }
        }

        public ObservableCollection<Photo> Photos
        {
            get { return photos; }
            set { Set(ref photos, value); }
        }

        public Photo TitlePhoto
        {
            get
            {
                return Photos.FirstOrDefault();
            }
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
            set
            {
                if (Set(ref hasLimitedLifespan, value)) {
                    RaisePropertyChanged(() => HasInfiniteLifespan);
                }
            }
        }

        public bool HasInfiniteLifespan
        {
            get { return !HasLimitedLifespan; }
            set { HasLimitedLifespan = !value; }
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
            get
            {
                return Convert.ToInt32(DateRemoved.Subtract(DateTime.Today).TotalDays);
            }
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

        public Document Clone()
        {
            return new Document(id, category, dateAdded, lifespan, hasLimitedLifespan, tags, photos);
        }
    }
}
