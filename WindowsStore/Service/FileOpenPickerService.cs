using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyDocs.WindowsStore.Service
{
    public class FileOpenPickerService : IFileOpenPickerService
    {
        public async Task<IEnumerable<StorageFile>> PickSubDocuments()
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add("*");
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.ViewMode = PickerViewMode.List;
            return await filePicker.PickMultipleFilesAsync();
        }

        public async Task<StorageFile> PickImportFile()
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".zip");
            return await filePicker.PickSingleFileAsync();
        }
    }
}
