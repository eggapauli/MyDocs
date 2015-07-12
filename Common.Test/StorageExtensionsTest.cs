using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Common.Test
{
    [TestClass]
    public class StorageExtensionsTest
    {
        private IStorageFolder source;
        private IStorageFolder destination;

        [TestInitialize]
        public async Task Init()
        {
            source = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("source");
            destination = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("destination");
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            foreach (var folder in new[] { source, destination })
            {
                try
                {
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch { }
            }
        }

        [TestMethod]
        public async Task MoveRecursiveShouldWork()
        {
            await SetupSourceFolder();

            await source.MoveRecursive(destination);

            var files = await destination.GetFilesAsync();
            files.Select(f => f.Name).Should().BeEquivalentTo("a.txt", "b.txt");

            var folders = await destination.GetFoldersAsync();
            folders.Select(f => f.Name).Should().BeEquivalentTo("c");

            var folder = await destination.GetFolderAsync("c");
            var subFiles = await folder.GetFilesAsync();
            subFiles.Select(f => f.Name).Should().BeEquivalentTo("c.txt");
        }

        [TestMethod]
        public async Task MoveRecursiveShouldMergeIntoExistingFolder()
        {
            await SetupSourceFolder();

            await destination.CreateFileAsync("x.txt");
            var subFolder = await destination.CreateFolderAsync("c");
            await subFolder.CreateFileAsync("y.txt");

            await source.MoveRecursive(destination);

            var files = await destination.GetFilesAsync();
            files.Select(f => f.Name).Should().BeEquivalentTo("a.txt", "b.txt", "x.txt");

            var folders = await destination.GetFoldersAsync();
            folders.Select(f => f.Name).Should().BeEquivalentTo("c");

            var folder = await destination.GetFolderAsync("c");
            var subFiles = await folder.GetFilesAsync();
            subFiles.Select(f => f.Name).Should().BeEquivalentTo("c.txt", "y.txt");
        }

        [TestMethod]
        public async Task MoveRecursiveShouldDeleteSourceFolder()
        {
            await SetupSourceFolder();

            await source.MoveRecursive(destination);

            new Func<Task>(async () => await StorageFolder.GetFolderFromPathAsync(source.Path))
                .ShouldThrow<FileNotFoundException>();
        }

        private async Task SetupSourceFolder()
        {
            await source.CreateFileAsync("a.txt");
            await source.CreateFileAsync("b.txt");
            var subFolder = await source.CreateFolderAsync("c");
            await subFolder.CreateFileAsync("c.txt");
        }
    }
}
