using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using System;
using System.Threading.Tasks;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class EditDocumentViewModelTest
    {
        [TestMethod]
        public void AddPhotoFromCameraCommandShouldBeEnabled()
        {
            var sut = CreateSut();

            sut.AddPhotoFromCameraCommand.CanExecute(null).Should().BeTrue();
        }

        //[TestMethod]
        //public void ShouldAddPhotoFromCamera()
        //{
        //    var testFile = // TODO create StorageFile
        //    var cameraService = A.Fake<ICameraService>();
        //    A.CallTo(() => cameraService.GetPhotoForDocumentAsync(A<Document>._)).Returns(new Photo(testFile));
        //    var sut = CreateSut();
        //    sut.EditingDocument = new Document();

        //    sut.AddPhotoFromCameraCommand.Execute(null);
        //    WaitForCommand();
        //    sut.EditingDocument.SubDocuments.Count.Should().Be(1);
        //}

        [TestMethod]
        public void ShouldNotAddNullPhoto()
        {
            var cameraService = A.Fake<ICameraService>();
            A.CallTo(() => cameraService.GetPhotoForDocumentAsync(A<Document>._)).Returns<Photo>(null);
            var sut = CreateSut();
            sut.EditingDocument = new Document();

            sut.AddPhotoFromCameraCommand.Execute(null);
            WaitForCommand();
            sut.EditingDocument.SubDocuments.Should().BeEmpty();
        }

        [TestMethod]
        public void ShouldNotAddPhotoFromCameraWhenServiceCallFails()
        {
            var cameraService = A.Fake<ICameraService>();
            A.CallTo(() => cameraService.GetPhotoForDocumentAsync(A<Document>._)).Throws<Exception>();
            var sut = CreateSut();
            sut.EditingDocument = new Document();

            sut.AddPhotoFromCameraCommand.Execute(null);
            WaitForCommand();
            sut.EditingDocument.SubDocuments.Should().BeEmpty();
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileAddingPhotoFromCamera()
        {
            var tcs = new TaskCompletionSource<Photo>();
            var cameraService = A.Fake<ICameraService>();
            A.CallTo(() => cameraService.GetPhotoForDocumentAsync(A<Document>._)).Returns(tcs.Task);
            var sut = CreateSut(cameraService: cameraService);

            sut.IsBusy.Should().BeFalse();
            sut.AddPhotoFromCameraCommand.Execute(null);
            WaitForCommand();
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            WaitForCommand();
            sut.IsBusy.Should().BeFalse();
        }
    }
}
