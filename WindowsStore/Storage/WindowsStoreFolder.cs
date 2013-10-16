using MyDocs.Common.Contract.Storage;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Storage
{
    public class WindowsStoreFolder : IFolder
    {
        public StorageFolder Folder { get; private set; }

        public string Path
        {
            get { return Folder.Path; }
        }

        public WindowsStoreFolder(StorageFolder folder)
        {
            Folder = folder;
        }

        public async Task<IFile> CreateFileAsync(string name)
        {
            return new WindowsStoreFile(await Folder.CreateFileAsync(name));
        }
    }
}
