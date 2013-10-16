using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDocs.Common.Model
{
    public class Photo : ObservableObject
    {
        private string title;
        private IFile file;
        private IEnumerable<IFile> previews;

        public string Title
        {
            get { return title ?? File.Name; }
            private set { Set(ref title, value); }
        }

        public IFile File
        {
            get { return file; }
            private set { Set(ref file, value); }
        }

        public IEnumerable<IFile> Previews
        {
            get { return previews; }
            private set { Set(ref previews, value); }
        }

        public IEnumerable<IFile> Files
        {
            get
            {
                foreach (var preview in previews) {
                    yield return preview;
                }
                yield return file;
            }
        }

        public Photo(string title, IFile file, IEnumerable<IFile> previews = null)
        {
            this.title = title;
            this.file = file;
            this.previews = previews ?? new List<IFile>();
        }
    }
}
