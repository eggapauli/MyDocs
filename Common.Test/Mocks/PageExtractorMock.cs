using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyDocs.Common.Model.Logic;
using Windows.Storage;

namespace Common.Test.Mocks
{
    class PageExtractorMock : IPageExtractor
    {
        public Task<IEnumerable<Photo>> ExtractPages(StorageFile file, Document document)
        {
            throw new NotImplementedException();
        }

        public bool SupportsExtension(string extension)
        {
            throw new NotImplementedException();
        }
    }
}
