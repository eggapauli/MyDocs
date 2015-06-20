using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Windows.Storage;

namespace MyDocs.Common.Model.Logic
{
    public class SubDocument
    {
        public string Title { get; private set; }

        public IStorageFile File { get; private set; }

        public IImmutableList<Photo> Photos { get; private set; }

        public SubDocument(IStorageFile file, IEnumerable<Photo> photos)
            : this(Path.GetFileNameWithoutExtension(file.Name), file, photos)
        { }

        public SubDocument(string title, IStorageFile file, IEnumerable<Photo> photos)
        {
            Title = title;
            File = file;
            Photos = photos.ToImmutableList();
        }
    }
}
