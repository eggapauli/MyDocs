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
        public async Task<IEnumerable<IFile>> PickFilesForDocumentAsync(Document document)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add("*");
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.ViewMode = PickerViewMode.List;

            var files = await filePicker.PickMultipleFilesAsync();
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(document.Id.ToString(), CreationCollisionOption.OpenIfExists);
            var tasks = files.Select(file => file.CopyAsync(folder, file.Name, NameCollisionOption.GenerateUniqueName).AsTask());
            var copies = await Task.WhenAll(tasks);
            return copies.Select(file => new WindowsStoreFile(file));
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
