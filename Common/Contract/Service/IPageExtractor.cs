using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface IPageExtractor
    {
        bool SupportsExtension(string extension);

        Task<IEnumerable<Photo>> ExtractPages(StorageFile file, Document document);
    }

    public class ImagePageExtractor : IPageExtractor
    {
        public bool SupportsExtension(string extension)
        {
            return new[] { ".bmp", ".gif", ".jpeg", ".jpg", ".png" }.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public Task<IEnumerable<Photo>> ExtractPages(StorageFile file, Document document)
        {
            return Task.FromResult<IEnumerable<Photo>>(new[] { new Photo(file) });
        }
    }

    public class PageExtractorList : IPageExtractor
    {
        private readonly IList<IPageExtractor> extractors;

        public PageExtractorList(IEnumerable<IPageExtractor> extractors)
        {
            this.extractors = extractors.ToList();
        }

        public bool SupportsExtension(string extension)
        {
            return extractors.Any(e => e.SupportsExtension(extension));
        }

        public async Task<IEnumerable<Photo>> ExtractPages(StorageFile file, Document document)
        {
            var extension = Path.GetExtension(file.Name);
            return await extractors
                .Single(e => e.SupportsExtension(extension))
                .ExtractPages(file, document);
        }
    }
}
