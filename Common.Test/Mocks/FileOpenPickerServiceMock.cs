using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyDocs.Common.Model.View;
using Windows.Storage;
using System.Linq;

namespace Common.Test.Mocks
{
    class FileOpenPickerServiceMock : IFileOpenPickerService
    {
        public Func<Document, Task<IEnumerable<StorageFile>>> PickFilesForDocumentFunc =
           delegate { return Task.FromResult(Enumerable.Empty<StorageFile>()); };
        public Task<IEnumerable<StorageFile>> PickFilesForDocumentAsync(Document document)
        {
            return PickFilesForDocumentFunc(document);
        }

        public Task<StorageFile> PickOpenFileAsync(IEnumerable<string> fileTypes)
        {
            throw new NotImplementedException();
        }
    }
}
