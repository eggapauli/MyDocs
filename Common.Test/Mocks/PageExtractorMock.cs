using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyDocs.Common.Model.Logic;
using Windows.Storage;

namespace Common.Test.Mocks
{
    class PageExtractorMock : IPageExtractor
    {
        public Func<StorageFile, Document, Task<IEnumerable<Photo>>> ExtractPagesFunc =
            delegate { return Task.FromResult(Enumerable.Empty<Photo>()); };
        public Task<IEnumerable<Photo>> ExtractPages(StorageFile file, Document document)
        {
            return ExtractPagesFunc(file, document);
        }

        public Func<string, bool> SupportsExtensionFunc = delegate { return true; };
        public bool SupportsExtension(string extension)
        {
            return SupportsExtensionFunc(extension);
        }
    }
}
