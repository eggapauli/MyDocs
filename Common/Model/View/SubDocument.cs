using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Model.View
{
    public class SubDocument : ObservableObject
    {
        private readonly string title;
        private readonly IFile file;
        private IImmutableList<Photo> photos;

        public string Title
        {
            get { return title; }
        }

        public IFile File
        {
            get { return file; }
        }

        public IImmutableList<Photo> Photos
        {
            get { return photos; }
            set { Set(ref photos, value); }
        }

        public SubDocument(IFile file, IEnumerable<Photo> photos)
        {
            this.file = file;
            Photos = photos.ToImmutableList();
            title = Path.GetFileNameWithoutExtension(file.Name);
        }

        public void RemovePhoto(Photo photo)
        {
            Photos = Photos.Remove(photo);
        }

        public Logic.SubDocument ToLogic()
        {
            return new Logic.SubDocument(Title, File, Photos.Select(p => p.ToLogic()));
        }

        public static SubDocument FromLogic(Logic.SubDocument subDocument)
        {
            return new SubDocument(subDocument.File, subDocument.Photos.Select(Photo.FromLogic));
        }
    }
}
