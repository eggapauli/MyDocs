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
            get { return title; }
            set
            {
                if (title != value) {
                    title = value;
                    RaisePropertyChanged(() => Title);
                }
            }
        }

        public IFile File
        {
            get { return file; }
            set
            {
                if (file != value) {
                    file = value;
                    RaisePropertyChanged(() => File);
                }
            }
        }

        public IFile Preview
        {
            get { return preview ?? file; }
            set
            {
                if (preview != value) {
                    preview = value;
                    RaisePropertyChanged(() => Preview);
                }
            }
        }

        public Photo(string title, IFile file, IFile preview = null)
        {
            Title = title;
            File = file;
            Preview = preview;
        }
    }
}
