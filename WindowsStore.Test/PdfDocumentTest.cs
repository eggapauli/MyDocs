using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.ApplicationModel;

namespace WindowsStore.Test
{
    [TestClass]
    public class PdfDocumentTest
    {
        [TestMethod]
        public async Task ShouldThrowExceptionWhenFileHasPassword()
        {
            var docFolder = await Package.Current.InstalledLocation.GetFolderAsync("PdfTest");
            var file = await docFolder.GetFileAsync("ProtectedDocument.pdf");
            try {
                await PdfDocument.LoadFromFileAsync(file);
                Assert.Fail();
            }
            catch (Exception e) {
                Assert.AreEqual(typeof(Exception), e.GetType());
                const string key = "RestrictedDescription";
                Assert.IsTrue(e.Data.Contains(key));
                Assert.AreEqual(e.Data[key], "Unable to update the password. The value provided as the current password is incorrect.\r\n");
            }
        }

        [TestMethod]
        public async Task ShouldLoadDocument()
        {
            var docFolder = await Package.Current.InstalledLocation.GetFolderAsync("PdfTest");
            var file = await docFolder.GetFileAsync("ProtectedDocument.pdf");
            try {
                await PdfDocument.LoadFromFileAsync(file);
                Assert.Fail();
            }
            catch (Exception e) {
                Assert.AreEqual(typeof(Exception), e.GetType());
            }

            var doc = await PdfDocument.LoadFromFileAsync(file, "2804040488");
            Assert.IsTrue(doc.IsPasswordProtected);
        }
    }
}
