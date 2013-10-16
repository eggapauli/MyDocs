using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.WindowsStore.Storage;
using System;
using System.Threading.Tasks;
using Windows.Media.Capture;

namespace MyDocs.WindowsStore.Service
{
	public class CameraService : ICameraService
	{
		public async Task<IFile> CaptureFileAsync()
		{
			CameraCaptureUI camera = new CameraCaptureUI();
			var file = await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);
			if (file == null) {
				return null;
			}
			return new WindowsStoreFile(file);
		}
	}
}
