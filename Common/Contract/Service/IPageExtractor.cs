using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IPageExtractor
    {
        IEnumerable<string> SupportedExtensions { get; }

        Task<IEnumerable<IFile>> ExtractPages(IFile file);
    }

    public class PageExtractorList : IPageExtractor
    {
        private readonly List<IPageExtractor> extractors;

        public PageExtractorList(IEnumerable<IPageExtractor> extractors)
        {
            this.extractors = extractors.ToList();
        }

        public IEnumerable<string> SupportedExtensions
        {
            get { return extractors.SelectMany(e => e.SupportedExtensions); }
        }

        public async Task<IEnumerable<IFile>> ExtractPages(IFile file)
        {
            var extension = Path.GetExtension(file.Name);
            return await extractors
                .Single(e => e.SupportedExtensions.Contains(extension))
                .ExtractPages(file);
        }
    }
}
