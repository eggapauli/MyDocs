using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Linq;

namespace MyDocs.Common.Model
{
    public class Category : ObservableObject
    {
        private string name;
        private IEnumerable<Document> documents;

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public IEnumerable<Document> Documents
        {
            get { return documents; }
            private set { Set(ref documents, value); }
        }

        public int CountDocuments
        {
            get { return Documents.OfType<Document>().Count(); }
        }

        public Photo TitlePhoto
        {
            get { return Documents.OfType<Document>().First().TitlePhoto; }
        }

        public Category(string name, IEnumerable<Document> documents = null)
        {
            Name = name;
            Documents = documents;
        }
    }
}
