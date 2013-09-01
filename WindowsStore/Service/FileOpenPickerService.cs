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
    public class FileOpenPickerService : IFileOpenPickerService
    {
        private IUserInterfaceService uiService;

        public FileOpenPickerService(IUserInterfaceService uiService)
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
            bool error = false;
            foreach (var file in files) {
                if (file.FileType.ToLower() == ".pdf") {
                    // TODO ask the user for a password if the file is password-protected
                    PdfDocument doc = null;
                    try {
                        doc = await PdfDocument.LoadFromFileAsync(file);

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
                    catch (Exception) {
                        error = true;
                    }
                }
                else {
                    var copy = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.GenerateUniqueName);
                    photos.Add(new Photo(copy.DisplayName, new WindowsStoreFile(copy)));
                }
            }
            if (error) {
                await uiService.ShowErrorAsync("fileLoadError");
            }
            return photos;
        }
    }
}
