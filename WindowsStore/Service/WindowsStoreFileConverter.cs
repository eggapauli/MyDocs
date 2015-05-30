using MyDocs.Common.Contract.Service;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class WindowsStoreFileConverter : IFileConverter
    {
        public async Task<StorageFile> ToFile(Uri uri)
        {
            return await StorageFile.GetFileFromApplicationUriAsync(uri);
        }
    }
}
