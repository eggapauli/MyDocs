using mupdfwinrt;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
					var doc = new mudocument();
					await doc.OpenFileAsync(file);
					if (doc.RequiresPassword()) {
						//doc.ApplyPassword(...);
						await uiService.ShowErrorAsync("protectedDocumentError");
						continue;
					}
					var copy = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.GenerateUniqueName);
					var wrappedFile = new WindowsStoreFile(copy);

					for (int i = 0; i < doc.GetNumPages(); i++) {
						var size = doc.GetPageSize(i);
						var page = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(copy.DisplayName + ".jpg", CreationCollisionOption.GenerateUniqueName);
						using (var stream = await doc.RenderPageAsync(i, (int)size.X, (int)size.Y, false)) {
							DataReader dataReader = new DataReader(stream);
							await dataReader.LoadAsync((uint)stream.Size);
							byte[] buffer = new byte[(int)stream.Size];
							dataReader.ReadBytes(buffer);

							await FileIO.WriteBytesAsync(page, buffer);
						}
						photos.Add(new Photo(copy.DisplayName, wrappedFile, new WindowsStoreFile(page)));
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
