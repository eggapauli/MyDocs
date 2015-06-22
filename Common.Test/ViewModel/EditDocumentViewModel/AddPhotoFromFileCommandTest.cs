using Common.Test.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class EditDocumentViewModelTest
    {
        [TestMethod]
        public void AddPhotoFromFileCommandShouldBeEnabled()
        {
            var sut = CreateSut();

            sut.AddPhotoFromFileCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void ShouldAddPickedFileAsSubDocument()
        {
            var file = new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png");
            var filePicker = CreateFilePickerMock(new[] { file });
            var pageExtractor = CreatePageExtractorMock(1);

            var sut = CreateSut(filePicker: filePicker, pageExtractor: pageExtractor);
            sut.EditingDocument = new Document();
            sut.AddPhotoFromFileCommand.Execute(null);
            WaitForCommand();

            sut.EditingDocument.SubDocuments.Count.Should().Be(1);
        }

        [TestMethod]
        public void ShouldAddExtractedPhotosFromPickedFiles()
        {
            var file = new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png");
            var filePicker = CreateFilePickerMock(new[] { file });
            var pageExtractor = CreatePageExtractorMock(2);

            var sut = CreateSut(filePicker: filePicker, pageExtractor: pageExtractor);
            sut.EditingDocument = new Document();
            sut.AddPhotoFromFileCommand.Execute(null);
            WaitForCommand();

            sut.EditingDocument
                .SubDocuments
                .SelectMany(sd => sd.Photos)
                .Count()
                .Should()
                .Be(2);
        }

        [TestMethod]
        public void ShouldSkipFilesWherePageExtractionFails()
        {
            var files = new[]
            {
                new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png"),
                new Uri("ms-appx:///Images/UnitTestSmallLogo.scale-100.png")
            };
            var filePicker = CreateFilePickerMock(files);
            var pageExtractor = new PageExtractorMock
            {
                SupportsExtensionFunc = delegate { return true; },
                ExtractPagesFunc = (f, _) =>
                {
                    if (f.Name == files[0].Segments.Last())
                    {
                        throw new Exception("Test");
                    }
                    var pages = Enumerable.Repeat(new Model.Logic.Photo(f), 1);
                    return Task.FromResult(pages);
                }
            };

            var sut = CreateSut(filePicker: filePicker, pageExtractor: pageExtractor);
            sut.EditingDocument = new Document();
            sut.AddPhotoFromFileCommand.Execute(null);
            WaitForCommand();

            sut.EditingDocument.SubDocuments.Count.Should().Be(1);
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileExtractingPages()
        {
            var file = new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png");
            var filePicker = CreateFilePickerMock(new[] { file });
            var tcs = new TaskCompletionSource<IEnumerable<Model.Logic.Photo>>();
            var pageExtractor = new PageExtractorMock
            {
                SupportsExtensionFunc = delegate { return true; },
                ExtractPagesFunc = delegate { return tcs.Task; }
            };

            var sut = CreateSut(filePicker: filePicker, pageExtractor: pageExtractor);
            sut.EditingDocument = new Document();
            sut.IsBusy.Should().BeFalse();

            sut.AddPhotoFromFileCommand.Execute(null);
            WaitForCommand();
            sut.IsBusy.Should().BeTrue();

            tcs.SetResult(Enumerable.Empty<Model.Logic.Photo>());
            WaitForCommand();
            sut.IsBusy.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldNotAddAnythingWhenUserDidntPickFiles()
        {
            var filePicker = new FileOpenPickerServiceMock();
            filePicker.PickFilesForDocumentFunc =
                delegate { return Task.FromResult(Enumerable.Empty<StorageFile>()); };
            var sut = CreateSut(filePicker: filePicker);
            sut.EditingDocument = new Document();

            sut.AddPhotoFromFileCommand.Execute(null);
            WaitForCommand();
            sut.EditingDocument.SubDocuments.Should().BeEmpty();
        }

        [TestMethod]
        public void ShouldNotAddPhotoFromFileWhenServiceCallFails()
        {
            var filePicker = new FileOpenPickerServiceMock();
            filePicker.PickFilesForDocumentFunc =
                delegate { throw new Exception("Test"); };
            var sut = CreateSut();
            sut.EditingDocument = new Document();

            sut.AddPhotoFromFileCommand.Execute(null);
            WaitForCommand();
            sut.EditingDocument.SubDocuments.Should().BeEmpty();
        }

        private FileOpenPickerServiceMock CreateFilePickerMock(IEnumerable<Uri> fileUris)
        {
            return new FileOpenPickerServiceMock
            {
                PickFilesForDocumentFunc = async delegate
                {
                    var tasks = fileUris
                        .Select(StorageFile.GetFileFromApplicationUriAsync)
                        .Select(x => x.AsTask());
                    return await Task.WhenAll(tasks);
                }
            };
        }

        private PageExtractorMock CreatePageExtractorMock(int pagesPerFile)
        {
            return new PageExtractorMock
            {
                SupportsExtensionFunc = delegate { return true; },
                ExtractPagesFunc = (f, _) =>
                {
                    var pages = Enumerable.Repeat(new Model.Logic.Photo(f), pagesPerFile);
                    return Task.FromResult(pages);
                }
            };
        }
    }
}
