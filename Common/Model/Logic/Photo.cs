using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Model.Logic
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
    }
}
