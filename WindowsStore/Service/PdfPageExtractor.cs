using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class PdfPageExtractor : IPageExtractor
    {
        public IEnumerable<string> SupportedExtensions
        {
            get { yield return ".pdf"; }
        }

        public async Task<IEnumerable<IFile>> ExtractPages(IFile file)
        {
            // TODO ask the user for a password if the file is password-protected
            var doc = await PdfDocument.LoadFromFileAsync(((WindowsStoreFile)file).File);

            var extractTasks = Enumerable.Range(0, (int)doc.PageCount)
                .Select(i => ExtractPage(doc, i));
            var images = await Task.WhenAll(extractTasks);
            return images.Select(image => new WindowsStoreFile(image));
        }

        private async Task<StorageFile> ExtractPage(PdfDocument doc, int pageNumber)
        {
            var image = await ApplicationData.Current.TemporaryFolder
                .CreateFileAsync(Guid.NewGuid().ToString() + ".jpg", CreationCollisionOption.GenerateUniqueName);
            using (var page = doc.GetPage((uint)pageNumber))
            using (var stream = await image.OpenAsync(FileAccessMode.ReadWrite)) {
                await page.RenderToStreamAsync(stream);
            }
            return image;
        }
    }
}
