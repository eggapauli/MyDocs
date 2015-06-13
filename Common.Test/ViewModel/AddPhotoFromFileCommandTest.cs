using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using System;
using System.Collections.Generic;

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

        //[TestMethod]
        //public void ShouldNotAddAnythingWhenUserDidntPickFiles()
        //{
        //    var filePicker = A.Fake<IFileOpenPickerService>();
        //    A.CallTo(() => filePicker.PickFilesForDocumentAsync(A<Document>._)).Returns(new List<Photo>());
        //    var sut = CreateSut();
        //    sut.EditingDocument = new Document();

        //    sut.AddPhotoFromFileCommand.Execute(null);
        //    WaitForCommand();
        //    sut.EditingDocument.SubDocuments.Should().BeEmpty();
        //}

        //[TestMethod]
        //public void ShouldNotAddPhotoFromFileWhenServiceCallFails()
        //{
        //    var filePicker = A.Fake<IFileOpenPickerService>();
        //    A.CallTo(() => filePicker.PickFilesForDocumentAsync(A<Document>._)).Throws<Exception>();
        //    var sut = CreateSut();
        //    sut.EditingDocument = new Document();

        //    sut.AddPhotoFromFileCommand.Execute(null);
        //    WaitForCommand();
        //    sut.EditingDocument.SubDocuments.Should().BeEmpty();
        //}
    }
}
