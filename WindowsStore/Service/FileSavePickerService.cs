using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyDocs.WindowsStore.Service
{
    public class FileSavePickerService : IFileSavePickerService
    {
        public async Task<IStorageFile> PickSaveFileAsync(string suggestedFileName, IDictionary<string, IList<string>> fileTypes)
        {
            var filePicker = new FileSavePicker { SuggestedFileName = suggestedFileName };
            foreach (var fileType in fileTypes) {
                filePicker.FileTypeChoices.Add(fileType);
            }
            return await filePicker.PickSaveFileAsync();
        }
    }
}
