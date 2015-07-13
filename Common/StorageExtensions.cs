using MyDocs.Common.Contract.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.Common
{
    public static class StorageExtensions
    {
        private const string localFolderName = "local";
        private const string tempFolderName = "temp";
        private const string roamingFolderName = "roaming";

        public static string GetRelativePath(this IStorageFile storageFile)
        {
            string folderPath;
            if (storageFile.IsInFolder(ApplicationData.Current.LocalFolder))
            {
                folderPath = ApplicationData.Current.LocalFolder.Path;
            }
            else if (storageFile.IsInFolder(ApplicationData.Current.TemporaryFolder))
            {
                folderPath = ApplicationData.Current.TemporaryFolder.Path;
            }
            else if (storageFile.IsInFolder(ApplicationData.Current.RoamingFolder))
            {
                folderPath = ApplicationData.Current.RoamingFolder.Path;
            }
            else
            {
                throw new NotSupportedException("Unknown file location.");
            }
            return storageFile.Path.Substring(folderPath.Length + 1);
        }

        public static Uri GetUri(this IStorageFile storageFile)
        {
            string folderPath;
            if (storageFile.IsInFolder(ApplicationData.Current.LocalFolder))
            {
                folderPath = localFolderName;
            }
            else if (storageFile.IsInFolder(ApplicationData.Current.TemporaryFolder))
            {
                folderPath = tempFolderName;
            }
            else if (storageFile.IsInFolder(ApplicationData.Current.RoamingFolder))
            {
                folderPath = roamingFolderName;
            }
            else
            {
                throw new NotSupportedException("Unknown file location.");
            }
            return new Uri(string.Format("ms-appdata:///{0}/{1}", folderPath, storageFile.GetRelativePath()));
        }

        public static bool IsInFolder(this IStorageFile storageFile, IStorageFolder folder)
        {
            // TODO add '/' at the end of both?
            return System.IO.Path.GetDirectoryName(storageFile.Path).StartsWith(folder.Path);
        }

        public static bool IsInLocalFolder(this Uri fileUri)
        {
            return IsInFolder(fileUri, localFolderName);
        }

        private static bool IsInFolder(Uri fileUri, string folderName)
        {
            return fileUri.Segments[1].Trim('/') == folderName;
        }

        public static async Task<BitmapImage> GetResizedBitmapImageAsync(this StorageFile storageFile, FileSize fileSize = FileSize.Small)
        {
            int size;
            switch (fileSize)
            {
                case FileSize.Big: size = (int)Math.Max(Window.Current.Bounds.Width, Window.Current.Bounds.Height); break;
                case FileSize.Small:
                default: size = 250; break;
            }

            if (storageFile.IsImage())
            {
                var bmp = new BitmapImage(new Uri(storageFile.Path));
                if (bmp.PixelWidth > bmp.PixelHeight)
                {
                    bmp.DecodePixelWidth = size;
                }
                else
                {
                    bmp.DecodePixelHeight = size;
                }
                return bmp;
            }
            else
            {
                var bmp = new BitmapImage();
                using (StorageItemThumbnail thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)size))
                {
                    await bmp.SetSourceAsync(thumbnail);
                }
                return bmp;
            }
        }

        public static bool IsImage(this IStorageItem file)
        {
            return new[] { ".bmp", ".gif", ".jpeg", ".jpg", ".png" }.Contains(Path.GetExtension(file.Path), StringComparer.OrdinalIgnoreCase);
        }
    }
}
