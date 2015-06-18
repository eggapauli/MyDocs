using MyDocs.Common.Contract.Service;
using System.Threading.Tasks;
using MyDocs.Common.Model.View;
using System;

namespace Common.Test.Mocks
{
    class CameraServiceMock : ICameraService
    {
        public Func<Document, Task<Photo>> GetPhotoForDocumentFunc =
            delegate { return Task.FromResult<Photo>(null); };
        public Task<Photo> GetPhotoForDocumentAsync(Document document)
        {
            return GetPhotoForDocumentFunc(document);
        }
    }
}
