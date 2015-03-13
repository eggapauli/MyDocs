using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IFileOpenPickerService
    {
        Task<IEnumerable<IFile>> PickFilesForDocumentAsync(Document document);

        Task<IFile> PickOpenFileAsync(IEnumerable<string> fileTypes);
    }
}
