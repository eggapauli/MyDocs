using MyDocs.Common.Model.View;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface IFileOpenPickerService
    {
        Task<IEnumerable<StorageFile>> PickFilesForDocumentAsync(Document document);

        Task<StorageFile> PickOpenFileAsync(IEnumerable<string> fileTypes);
    }
}
