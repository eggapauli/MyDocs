using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model.Logic;
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

        Task<IEnumerable<Photo>> ExtractPages(IFile file, Document document);
    }

    public class ImagePageExtractor : IPageExtractor
    {

        public IEnumerable<string> SupportedExtensions
        {
            get
            {
                yield return ".bmp";
                yield return ".gif";
                yield return ".jpeg";
                yield return ".jpg";
                yield return ".png";
            }
        }

        public Task<IEnumerable<Photo>> ExtractPages(IFile file, Document document)
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

        public IEnumerable<string> SupportedExtensions
        {
            get { return extractors.SelectMany(e => e.SupportedExtensions); }
        }

        public async Task<IEnumerable<Photo>> ExtractPages(IFile file, Document document)
        {
            var extension = Path.GetExtension(file.Name);
            return await extractors
                .Single(e => e.SupportedExtensions.Contains(extension))
                .ExtractPages(file, document);
        }
    }
}
