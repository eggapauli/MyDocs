using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace MyDocs.WindowsStore.Service
{
    public class FileSavePickerService : IFileSavePickerService
    {
        public async Task<IFile> PickSaveFileAsync(string suggestedFileName, IDictionary<string, IList<string>> fileTypes)
        {
            var filePicker = new FileSavePicker { SuggestedFileName = suggestedFileName };
            foreach (var fileType in fileTypes) {
                filePicker.FileTypeChoices.Add(fileType);
            }
            var file = await filePicker.PickSaveFileAsync();
            return file != null ? new WindowsStoreFile(file) : null;
        }
    }
}
