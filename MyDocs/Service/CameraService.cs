using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.WindowsStoreFrontend.Storage;
using System;
using System.Threading.Tasks;
using Windows.Media.Capture;

namespace MyDocs.WindowsStoreFrontend.Service
{
	public class CameraService : ICameraService
	{
		public async Task<IFile> CaptureFileAsync()
		{
			CameraCaptureUI camera = new CameraCaptureUI();
			return new WindowsStoreFile(await camera.CaptureFileAsync(CameraCaptureUIMode.Photo));
		}
	}
}
