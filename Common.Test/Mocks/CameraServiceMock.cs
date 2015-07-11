using MyDocs.Common.Contract.Service;
using System.Threading.Tasks;
using MyDocs.Common.Model.View;
using System;
using Windows.Storage;

namespace Common.Test.Mocks
{
    class CameraServiceMock : ICameraService
    {
        public Func<Task<StorageFile>> GetPhotoFunc =
            delegate { return Task.FromResult<StorageFile>(null); };
        public Task<StorageFile> GetPhoto()
        {
            return GetPhotoFunc();
        }
    }
}
