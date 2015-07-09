using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace WindowsStore.Test
{
    [TestClass]
    public class StorageFileTest
    {
        [TestMethod]
        public async Task ShouldGetAppxPathFromFileInLocalFolder()
        {
            await CreateAndGetFile(ApplicationData.Current.LocalFolder, "local");
        }

        [TestMethod]
        public async Task ShouldGetAppxPathFromFileInTemporaryFolder()
        {
            await CreateAndGetFile(ApplicationData.Current.TemporaryFolder, "temp");
        }

        private async Task CreateAndGetFile(StorageFolder folder, string path)
        {
            const string fileName = "test.pdf";
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var uri = new Uri(string.Format("ms-appdata:///{0}/{1}", path, fileName));
            await StorageFile.GetFileFromApplicationUriAsync(uri);
        }

        [TestMethod]
        public async Task ShouldThrowWhenFolderDoesntExist()
        {
            try {
                await ApplicationData.Current.LocalFolder.GetFolderAsync("xxx");
                Assert.Fail();
            }
            catch (Exception e) {
                Assert.IsInstanceOfType(e, typeof(FileNotFoundException));
            }
        }

        [TestMethod]
        public async Task CanDeleteNonEmptyFolder()
        {
            const string folderName = "testFolder";
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.ReplaceExisting);
            var file = await folder.CreateFileAsync("test.pdf", CreationCollisionOption.ReplaceExisting);
            await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            var folders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
            Assert.IsFalse(folders.Any(f => f.Name == folderName));
        }

        [TestMethod]
        public async Task ShouldGetBasicProperties()
        {
            const string fileName = "test.pdf";
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var basicProperties = await file.GetBasicPropertiesAsync();
            var properties = await basicProperties.RetrievePropertiesAsync(new string[0]);
            foreach (var property in properties) {
                var value = property.Value != null ? property.Value.ToString() : "<null>";
                Debug.WriteLine("Key: {0}, Value: {1}", property.Key, value);
            }
        }

        [TestMethod]
        public async Task ShouldCreateFileWithSlashes()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(@"testfolder\test.pdf", CreationCollisionOption.ReplaceExisting);
            var testFolder = await file.GetParentAsync();
            Assert.AreEqual("testfolder", testFolder.Name);
            var localFolder = await testFolder.GetParentAsync();
            Assert.AreEqual(ApplicationData.Current.LocalFolder.Name, localFolder.Name);
        }
    }
}
