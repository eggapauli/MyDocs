using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.Common.Contract.Storage
{
    public interface IFile
    {
        string Name { get; }
        string DisplayName { get; }
        string Path { get; }
        string GetRelativePath();
        Uri GetUri();
        bool IsInFolder(IFolder folder);
        Task<BitmapImage> GetResizedBitmapImageAsync(FileSize fileSize = FileSize.Small);
        Task MoveAsync(IFolder folder);
        Task MoveAsync(IFolder folder, string name);
        Task<IFile> CopyAsync(IFolder folder);
        Task<IFile> CopyAsync(IFolder folder, string name);
        Task DeleteAsync();
        Task<Stream> OpenReadAsync();
        Task<Stream> OpenWriteAsync();
    }
}
