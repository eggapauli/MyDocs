using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace MyDocs.WindowsStore.Service
{
	public class FilePickerService : IFilePickerService
	{
		public async Task<IEnumerable<IFile>> PickMultipleImagesAsync()
		{
			FileOpenPicker filePicker = new FileOpenPicker();
			filePicker.FileTypeFilter.Add(".png");
			filePicker.FileTypeFilter.Add(".jpg");
			filePicker.FileTypeFilter.Add(".jpeg");
			filePicker.FileTypeFilter.Add(".gif");
			filePicker.FileTypeFilter.Add(".bmp");
			filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			filePicker.ViewMode = PickerViewMode.List;
			return (await filePicker.PickMultipleFilesAsync()).Select(f => new WindowsStoreFile(f));
		}
	}
}
