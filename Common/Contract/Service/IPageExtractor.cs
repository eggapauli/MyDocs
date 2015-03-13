using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model;
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

        Task<IEnumerable<IFile>> ExtractPages(IFile file, Document document);
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

        public async Task<IEnumerable<IFile>> ExtractPages(IFile file, Document document)
        {
            var extension = Path.GetExtension(file.Name);
            return await extractors
                .Single(e => e.SupportedExtensions.Contains(extension))
                .ExtractPages(file, document);
        }
    }
}
