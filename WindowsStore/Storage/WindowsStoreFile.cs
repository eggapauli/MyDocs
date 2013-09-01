using MyDocs.WindowsStore.Common;
using MyDocs.Common.Contract.Storage;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.WindowsStore.Storage
{
    public class WindowsStoreFile : IFile
    {
        public StorageFile File { get; private set; }

        public WindowsStoreFile(StorageFile file)
        {
            File = file;
        }

        public string Name { get { return File.Name; } }
        public string Path { get { return File.Path; } }

        public string GetRelativePath()
        {
            string folderPath = String.Empty;
            if (IsInFolder(new WindowsStoreFolder(ApplicationData.Current.LocalFolder))) {
                folderPath = ApplicationData.Current.LocalFolder.Path;
            }
            else if (IsInFolder(new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder))) {
                folderPath = ApplicationData.Current.TemporaryFolder.Path;
            }
            else if (IsInFolder(new WindowsStoreFolder(ApplicationData.Current.RoamingFolder))) {
                folderPath = ApplicationData.Current.RoamingFolder.Path;
            }
            return File.Path.Substring(folderPath.Length + 1);
        }

        public bool IsInFolder(IFolder folder)
        {
            return System.IO.Path.GetDirectoryName(File.Path).StartsWith(folder.Path);
        }

        public async Task<IBitmapImage> GetResizedBitmapImageAsync(FileSize fileSize = FileSize.Small)
        {
            int size;
            switch (fileSize) {
                case FileSize.Big: size = (int)Math.Max(Window.Current.Bounds.Width, Window.Current.Bounds.Height); break;
                case FileSize.Small:
                default: size = 250; break;
            }

            BitmapImage bmp;
            if (File.IsImage()) {
                bmp = new BitmapImage(new Uri(File.Path));
                if (bmp.PixelWidth > bmp.PixelHeight) {
                    bmp.DecodePixelWidth = size;
                }
                else {
                    bmp.DecodePixelHeight = size;
                }
            }
            else {
                bmp = new BitmapImage();
                using (StorageItemThumbnail thumbnail = await File.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)size)) {
                    await bmp.SetSourceAsync(thumbnail);
                }
            }
            return new WindowsStoreBitmapImage(bmp, File.Name);
        }

        public async Task MoveAsync(IFolder folder)
        {
            await File.MoveAsync(((WindowsStoreFolder)folder).Folder);
        }

        public async Task MoveAsync(IFolder folder, string name)
        {
            await File.MoveAsync(((WindowsStoreFolder)folder).Folder, name);
        }

        public async Task<IFile> CopyAsync(IFolder folder, string name)
        {
            return await File.CopyAsync(((WindowsStoreFolder)folder).Folder, name).AsTask().ContinueWith(t => new WindowsStoreFile(t.Result));
        }

        public async Task<IFile> CopyAsync(IFolder folder)
        {
            return await File.CopyAsync(((WindowsStoreFolder)folder).Folder).AsTask().ContinueWith(t => new WindowsStoreFile(t.Result));
        }

        public async Task DeleteAsync()
        {
            await File.DeleteAsync();
        }

        public async Task<Stream> OpenReadAsync()
        {
            return (await File.OpenReadAsync()).AsStream();
        }

        public async Task<Stream> OpenWriteAsync()
        {
            return (await File.OpenAsync(FileAccessMode.ReadWrite)).AsStream();
        }
    }
}
