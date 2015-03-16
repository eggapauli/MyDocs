using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class WindowsStoreFileConverter : IFileConverter
    {
        public async Task<IFile> ToFile(Uri uri)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            return new WindowsStoreFile(file);
        }
    }
}
