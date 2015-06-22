using ReactiveUI;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MyDocs.Common.Model.View
{
    public class Category : ReactiveObject
    {
        private string name;
        private IImmutableList<Document> documents;

        public string Name
        {
            get { return name; }
            set { this.RaiseAndSetIfChanged(ref name, value); }
        }

        public IImmutableList<Document> Documents
        {
            get { return documents; }
            set { this.RaiseAndSetIfChanged(ref documents, value); }
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
