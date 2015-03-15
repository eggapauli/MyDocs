using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace MyDocs.Common.Model.Logic
{
    public class SubDocument
    {
        public string Title { get; private set; }

        public IFile File { get; private set; }

        public IImmutableList<Photo> Photos { get; private set; }

        public SubDocument(IFile file, IEnumerable<Photo> photos)
            : this(Path.GetFileNameWithoutExtension(file.Name), file, photos)
        { }

        public SubDocument(string title, IFile file, IEnumerable<Photo> photos)
        {
            Title = title;
            File = file;
            Photos = photos.ToImmutableList();
        }
    }
}
