using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model.Logic;
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
        public bool SupportsExtension(string extension)
        {
            return new[] { ".pdf" }.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IEnumerable<Photo>> ExtractPages(IFile file, Document document)
        {
            // TODO ask the user for a password if the file is password-protected
            var doc = await PdfDocument.LoadFromFileAsync(((WindowsStoreFile)file).File);

            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(document.Id.ToString(), CreationCollisionOption.OpenIfExists);
            var extractTasks = Enumerable.Range(0, (int)doc.PageCount)
                .Select(i => ExtractPage(doc, file.Name, i, folder));
            var images = await Task.WhenAll(extractTasks);
            return images.Select(image => new Photo(new WindowsStoreFile(image)));
        }

        private async Task<StorageFile> ExtractPage(PdfDocument doc, string fileName, int pageNumber, StorageFolder folder)
        {
            var pageFileName = Path.ChangeExtension(fileName, ".jpg");
            var image = await folder.CreateFileAsync(pageFileName, CreationCollisionOption.GenerateUniqueName);
            using (var page = doc.GetPage((uint)pageNumber))
            using (var stream = await image.OpenAsync(FileAccessMode.ReadWrite)) {
                await page.RenderToStreamAsync(stream);
            }
            return image;
        }
    }
}
