using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Model.View
{
    public class Photo
    {
        private readonly IFile file;

        public IFile File
        {
            get { return file; }
        }

        public Photo(IFile file)
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
