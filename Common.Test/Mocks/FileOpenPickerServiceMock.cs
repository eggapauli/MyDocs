using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace Common.Test.Mocks
{
    class FileOpenPickerServiceMock : IFileOpenPickerService
    {
        public Func<Task<IEnumerable<StorageFile>>> PickFilesForDocumentFunc =
           delegate { return Task.FromResult(Enumerable.Empty<StorageFile>()); };
        public Task<IEnumerable<StorageFile>> PickSubDocuments()
        {
            return PickFilesForDocumentFunc();
        }

        public Task<StorageFile> PickImportFile()
        {
            throw new NotImplementedException();
        }
    }
}
