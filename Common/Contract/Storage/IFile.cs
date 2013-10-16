using System.IO;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Storage
{
    public interface IFile
    {
        string Name { get; }
        string DisplayName { get; }
        string Path { get; }
        string GetRelativePath();
        bool IsInFolder(IFolder folder);
        Task<IBitmapImage> GetResizedBitmapImageAsync(FileSize fileSize = FileSize.Small);
        Task MoveAsync(IFolder folder);
        Task MoveAsync(IFolder folder, string name);
        Task<IFile> CopyAsync(IFolder folder);
        Task<IFile> CopyAsync(IFolder folder, string name);
        Task DeleteAsync();
        Task<Stream> OpenReadAsync();
        Task<Stream> OpenWriteAsync();
    }
}
