using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MyDocs.Common.Model.View
{
    public class Category : ObservableObject
    {
        private string name;
        private IImmutableList<Document> documents;

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public IImmutableList<Document> Documents
        {
            get { return documents; }
            set { Set(ref documents, value); }
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
            Documents = documents != null ? documents.ToImmutableList() : ImmutableList<Document>.Empty;
        }
    }
}
