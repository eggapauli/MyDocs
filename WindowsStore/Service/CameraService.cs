using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Threading.Tasks;
using Windows.Media.Capture;

namespace MyDocs.WindowsStore.Service
{
    public class CameraService : ICameraService
    {
        public async Task<Photo> CapturePhotoAsync()
        {
            var camera = new CameraCaptureUI();
            var file = await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file == null) {
                return null;
            }
            return new Photo(DateTime.Now.ToString("G"), new WindowsStoreFile(file));
        }
    }
}
