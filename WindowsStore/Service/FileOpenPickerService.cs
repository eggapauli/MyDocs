using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var files = await filePicker.PickMultipleFilesAsync();
            var folder = ApplicationData.Current.TemporaryFolder;
            var tasks = files.Select(file =>
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.Name);
                return file.CopyAsync(folder, fileName).AsTask();
            });
            return await Task.WhenAll(tasks);
        }

        public async Task<StorageFile> PickImportFile()
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".zip");
            return await filePicker.PickSingleFileAsync();
        }
    }
}
