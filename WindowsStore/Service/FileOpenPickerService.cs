using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyDocs.WindowsStore.Service
{
    public class FileOpenPickerService : IFileOpenPickerService
    {
        public async Task<IEnumerable<StorageFile>> PickFilesForDocumentAsync(Document document)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add("*");
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.ViewMode = PickerViewMode.List;

            var files = await filePicker.PickMultipleFilesAsync();
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(document.Id.ToString(), CreationCollisionOption.OpenIfExists);
            var tasks = files.Select(file => file.CopyAsync(folder, file.Name, NameCollisionOption.GenerateUniqueName).AsTask());
            return await Task.WhenAll(tasks);
        }

        public async Task<StorageFile> PickOpenFileAsync(IEnumerable<string> fileTypes)
        {
            var filePicker = new FileOpenPicker();
            foreach (var fileType in fileTypes) {
                filePicker.FileTypeFilter.Add(fileType);
            }
            return await filePicker.PickSingleFileAsync();
        }
    }
}
