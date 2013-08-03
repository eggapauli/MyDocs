using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace MyDocs.WindowsStore.Service
{
	public class FilePickerService : IFilePickerService
	{
		private IUserInterfaceService uiService;

		public FilePickerService(IUserInterfaceService uiService)
		{
			this.uiService = uiService;
		}

		public async Task<IEnumerable<Photo>> PickMultiplePhotosAsync()
		{
			FileOpenPicker filePicker = new FileOpenPicker();
			filePicker.FileTypeFilter.Add("*");
			filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			filePicker.ViewMode = PickerViewMode.List;

			var files = await filePicker.PickMultipleFilesAsync();
			var photos = new List<Photo>();
			foreach (var file in files) {
				if (file.FileType.ToLower() == ".pdf") {
					// TODO handle exception when doc is password protected
					var doc = await PdfDocument.LoadFromFileAsync(file);
					//if (doc.IsPasswordProtected) {
					//	string password = ...
					//	doc = await PdfDocument.LoadFromFileAsync(file, password);
					//}
					var copy = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.GenerateUniqueName);
					var wrappedFile = new WindowsStoreFile(copy);

					for (uint i = 0; i < doc.PageCount; i++) {
						var image = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(copy.DisplayName + ".jpg", CreationCollisionOption.GenerateUniqueName);
						using (var page = doc.GetPage(i))
						using (var stream = await image.OpenAsync(FileAccessMode.ReadWrite)) {
							await page.RenderToStreamAsync(stream);
						}

						photos.Add(new Photo(copy.DisplayName, wrappedFile, new WindowsStoreFile(image)));
					}
				}
				else {
					var copy = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.GenerateUniqueName);
					photos.Add(new Photo(copy.DisplayName, new WindowsStoreFile(copy)));
				}
			}
			return photos;
		}
	}
}
