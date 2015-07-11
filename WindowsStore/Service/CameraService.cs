using MyDocs.Common.Contract.Service;
using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class CameraService : ICameraService
    {
        public async Task<StorageFile> GetPhoto()
        {
            var camera = new CameraCaptureUI();
            return await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);
        }
    }
}
