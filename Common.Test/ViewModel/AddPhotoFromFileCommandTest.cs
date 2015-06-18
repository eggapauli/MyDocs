using Common.Test.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Contract.Service;
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
        public void ShouldAddExtractedPhotosFromPickedFiles()
        {
            throw new NotImplementedException();
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
    }
}
