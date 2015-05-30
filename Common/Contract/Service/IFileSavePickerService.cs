using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface IFileSavePickerService
    {
        Task<IStorageFile> PickSaveFileAsync(string suggestedFileName, IDictionary<string, IList<string>> fileTypes);
    }
}
