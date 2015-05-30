using Windows.Storage;

namespace MyDocs.Common.Model.View
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

        public Logic.Photo ToLogic()
        {
            return new Logic.Photo(File);
        }

        public static Photo FromLogic(Logic.Photo photo)
        {
            return new Photo(photo.File);
        }
    }
}
