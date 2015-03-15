﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
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
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(folderName);
            var file = await folder.CreateFileAsync("test.pdf");
            await folder.DeleteAsync();
            var folders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
            Assert.IsFalse(folders.Any(f => f.Name == folderName));
        }

        private async Task CreateAndGetFile(StorageFolder folder, string path)
        {
            var file = await folder.CreateFileAsync("test.pdf");
            ExceptionDispatchInfo exInfo = null;
            try {
                var uri = new Uri(string.Format("ms-appdata:///{0}/test.pdf", path));
                await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            catch (Exception e) {
                exInfo = ExceptionDispatchInfo.Capture(e);
            }

            await file.DeleteAsync();

            if (exInfo != null) {
                exInfo.Throw();
            }
        }
    }
}