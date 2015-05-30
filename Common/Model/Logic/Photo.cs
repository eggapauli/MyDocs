using Windows.Storage;

namespace MyDocs.Common.Model.Logic
{
    public class Photo
    {
        private readonly StorageFile file;

        public StorageFile File
        {
            get { return file; }
        }

        public Photo(StorageFile file)
        {
            this.file = file;
        }
    }
}
