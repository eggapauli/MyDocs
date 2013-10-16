using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
			set
			{
				if (id != value) {
					id = value;
					RaisePropertyChanged(() => Id);
				}
			}
		}

		public string Category
		{
			get { return category; }
			set
			{
				if (category != value) {
					category = value;
					RaisePropertyChanged(() => Category);
				}
			}
		}

		public ObservableCollection<Photo> Photos
		{
			get { return photos; }
			set
			{
				if (photos != value) {
					photos = value;
					RaisePropertyChanged(() => Photos);
				}
			}
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
				if (tags != value) {
					tags = value;
					RaisePropertyChanged(() => Tags);
					RaisePropertyChanged(() => TagsString);
				}
			}
		}

		public DateTime DateAdded
		{
			get { return dateAdded; }
			set
			{
				if (dateAdded != value) {
					dateAdded = value;
					RaisePropertyChanged(() => DateAdded);
				}
			}
		}

		public TimeSpan Lifespan
		{
			get { return lifespan; }
			set
			{
				if (lifespan != value) {
					lifespan = value;
					RaisePropertyChanged(() => Lifespan);
					RaisePropertyChanged(() => DateRemoved);
					RaisePropertyChanged(() => DateRemovedDay);
					RaisePropertyChanged(() => DateRemovedMonth);
					RaisePropertyChanged(() => DateRemovedYear);
					RaisePropertyChanged(() => DaysToRemoval);
				}
			}
		}

		public bool HasLimitedLifespan
		{
			get { return hasLimitedLifespan; }
			set
			{
				if (hasLimitedLifespan != value) {
					hasLimitedLifespan = value;
					RaisePropertyChanged(() => HasLimitedLifespan);
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
				RaisePropertyChanged(() => TagsString);
			}
		}

		public DateTime DateRemoved
		{
			get { return DateAdded.Add(Lifespan).Date; }
			set { Lifespan = value.Subtract(DateAdded); }
		}

		public int DateRemovedDay
		{
			get { return DateRemoved.Day; }
			set
			{
				int day = Math.Min(DateTime.DaysInMonth(DateRemoved.Year, DateRemoved.Month), value);
				DateRemoved = new DateTime(DateRemoved.Year, DateRemoved.Month, day);
			}
		}

		public int DateRemovedMonth
		{
			get { return DateRemoved.Month; }
			set
			{
				int day = Math.Min(DateTime.DaysInMonth(DateRemoved.Year, value), DateRemoved.Day);
				DateRemoved = new DateTime(DateRemoved.Year, value, day);
			}
		}

		public int DateRemovedYear
		{
			get { return DateRemoved.Year; }
			set
			{
				int day = Math.Min(DateTime.DaysInMonth(value, DateRemoved.Month), DateRemoved.Day);
				DateRemoved = new DateTime(value, DateRemoved.Month, day);
			}
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
			IEnumerable<Photo> photos = null
			)
		{
			Id = id;
			Category = category;
			DateAdded = dateAdded;
			Lifespan = lifespan;
			HasLimitedLifespan = hasLimitedLifespan;
			Tags = new ObservableCollection<string>(tags);

			if (photos != null) {
				Photos = new ObservableCollection<Photo>(photos);
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
