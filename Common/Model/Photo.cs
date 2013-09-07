using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Storage;
using System.Threading.Tasks;

namespace MyDocs.Common.Model
{
    public class Photo : ObservableObject
    {
        private string title;
        private IFile file;
        private IFile preview;

        public string Title
        {
            get { return title ?? File.Name; }
            set { Set(ref title, value); }
        }

        public IFile File
        {
            get { return file; }
            set { Set(ref file, value); }
        }

        public IFile Preview
        {
            get { return preview ?? file; }
            set { Set(ref preview, value); }
        }

        public Photo(string title, IFile file, IFile preview = null)
        {
            Title = title;
            File = file;
            Preview = preview;
        }
    }
}
