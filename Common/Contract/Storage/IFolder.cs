
using System.Threading.Tasks;
namespace MyDocs.Common.Contract.Storage
{
    public interface IFolder
    {
        string Path { get; }

        Task<IFile> CreateFileAsync(string name);
    }
}
