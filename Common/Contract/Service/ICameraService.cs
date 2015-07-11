using MyDocs.Common.Model.View;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface ICameraService
    {
        Task<StorageFile> GetPhoto();
    }
}
