using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<IEnumerable<IFile>> PickMultipleFilesAsync()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add("*");
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.ViewMode = PickerViewMode.List;

            var files = await filePicker.PickMultipleFilesAsync();

            var copies = new List<IFile>();
            foreach (var file in files) {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Name);
                var copy = await file.CopyAsync(ApplicationData.Current.TemporaryFolder, fileName);
                copies.Add(new WindowsStoreFile(copy));
            }
            return copies;
        }


        public async Task<IFile> PickOpenFileAsync(IEnumerable<string> fileTypes)
        {
            var filePicker = new FileOpenPicker();
            foreach (var fileType in fileTypes) {
                filePicker.FileTypeFilter.Add(fileType);
            }
            var file = await filePicker.PickSingleFileAsync();
            return file != null ? new WindowsStoreFile(file) : null;
        }
    }
}
