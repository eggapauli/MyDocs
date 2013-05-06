using MyDocs.Common;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.Model
{
	public class Category : ObservableObject
	{
		private ResourceLoader rl = new ResourceLoader();

		private string name;
		private SortedObservableCollection<Document> documents;

		public string Name
		{
			get { return name; }
			set
			{
				if (name != value) {
					name = value;
					RaisePropertyChanged(() => Name);
				}
			}
		}

		public SortedObservableCollection<Document> Documents
		{
			get { return documents; }
			set
			{
				if (documents != value) {
					documents = value;
					RaisePropertyChanged(() => Documents);
				}
			}
		}

		public string CountDocumentsText
		{
			get
			{
				int count = Documents.Count(d => !(d is AdDocument));
				return String.Format(rl.GetString("countDocumentsFormat"), count);
			}
		}

		public Photo TitlePhoto
		{
			get
			{
				Document doc = Documents.FirstOrDefault(d => !(d is AdDocument));
				if (doc != null) {
					return doc.TitlePhoto;
				}
				return null;
			}
		}

		public Category(string name, IEnumerable<Document> documents = null)
		{
			Name = name;
			if (documents != null) {
				Documents = new SortedObservableCollection<Document>(documents, new DocumentComparer());
			}
			else {
				Documents = new SortedObservableCollection<Document>(new DocumentComparer());
			}
		}
	}
}
