using GalaSoft.MvvmLight;
using MyDocs.Common.Collection;
using MyDocs.Common.Comparer;
using System.Collections.Generic;
using System.Linq;

namespace MyDocs.Common.Model
{
    public class Category : ObservableObject
    {
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
                    RaisePropertyChanged(() => CountDocuments);
                }
            }
        }

        public int CountDocuments
        {
            get
            {
                return Documents.Count(d => !(d is AdDocument));
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
