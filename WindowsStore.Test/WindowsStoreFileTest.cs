using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace WindowsStore.Test
{
    public class WindowsStoreFileTest
    {
        [TestClass]
        public class GetUri
        {
            [TestMethod]
            public async Task ShouldReturnCorrectMsAppDataUriForLocalFolder()
            {
                var folder = ApplicationData.Current.LocalFolder;
                await CreateFileAndTestUri(folder);
            }

            [TestMethod]
            public async Task ShouldReturnCorrectMsAppDataUriForLocalSubfolder()
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("testfolder", CreationCollisionOption.ReplaceExisting);
                await CreateFileAndTestUri(folder);
            }

            [TestMethod]
            public async Task ShouldReturnCorrectMsAppDataUriForTempFolder()
            {
                var folder = ApplicationData.Current.TemporaryFolder;
                await CreateFileAndTestUri(folder);
            }

            [TestMethod]
            public async Task ShouldReturnCorrectMsAppDataUriForRoamingFolder()
            {
                var folder = ApplicationData.Current.RoamingFolder;
                await CreateFileAndTestUri(folder);
            }

            private static async Task CreateFileAndTestUri(StorageFolder folder)
            {
                var storageFile = await folder.CreateFileAsync("test.pdf", CreationCollisionOption.ReplaceExisting);
                await StorageFile.GetFileFromApplicationUriAsync(storageFile.GetUri());
            }
        }
    }
}
