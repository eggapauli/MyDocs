using MyDocs.Common.Model.View;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface ICameraService
    {
        Task<Photo> GetPhotoForDocumentAsync(Document document);
    }
}
