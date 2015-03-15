using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class CameraService : ICameraService
    {
        public async Task<Photo> GetPhotoForDocumentAsync(Document document)
        {
            var camera = new CameraCaptureUI();
            var file = await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file == null) {
                return null;
            }
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(document.Id.ToString(), CreationCollisionOption.OpenIfExists);
            await file.MoveAsync(folder, DateTime.Now.ToString("R"), NameCollisionOption.GenerateUniqueName);
            return new Photo(new WindowsStoreFile(file));
        }
    }
}
