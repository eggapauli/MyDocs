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
    public class PdfService : IPdfService
    {
        public async Task<IEnumerable<IFile>> ExtractPages(IFile pdfFile)
        {
            var file = (WindowsStoreFile)pdfFile;
            var pages = new List<IFile>();

            // TODO ask the user for a password if the file is password-protected
            PdfDocument doc = await PdfDocument.LoadFromFileAsync(file.File);

            for (uint i = 0; i < doc.PageCount; i++) {
                var image = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(file.File.DisplayName + ".jpg", CreationCollisionOption.GenerateUniqueName);
                using (var page = doc.GetPage(i))
                using (var stream = await image.OpenAsync(FileAccessMode.ReadWrite)) {
                    await page.RenderToStreamAsync(stream);
                }

                pages.Add(new WindowsStoreFile(image));
            }

            return pages;
        }
    }
}
