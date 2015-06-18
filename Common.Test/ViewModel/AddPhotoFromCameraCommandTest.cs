using Common.Test.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using System;
using System.Threading.Tasks;
using Windows.Storage;

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

        [TestMethod]
        public async Task ShouldAddPhotoFromCamera()
        {
            var testFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png"));
            var cameraService = new CameraServiceMock();
            cameraService.GetPhotoForDocumentFunc = _ => Task.FromResult(new Photo(testFile));
            var sut = CreateSut(cameraService: cameraService);
            sut.EditingDocument = new Document();

            sut.AddPhotoFromCameraCommand.Execute(null);
            WaitForCommand();
            sut.EditingDocument.SubDocuments.Count.Should().Be(1);
        }

        [TestMethod]
        public void ShouldNotAddNullPhoto()
        {
            var cameraService = new CameraServiceMock();
            cameraService.GetPhotoForDocumentFunc = _ => Task.FromResult<Photo>(null);
            var sut = CreateSut(cameraService: cameraService);
            sut.EditingDocument = new Document();

            sut.AddPhotoFromCameraCommand.Execute(null);
            WaitForCommand();
            sut.EditingDocument.SubDocuments.Should().BeEmpty();
        }

        [TestMethod]
        public void ShouldNotAddPhotoFromCameraWhenServiceCallFails()
        {
            var cameraService = new CameraServiceMock();
            cameraService.GetPhotoForDocumentFunc = _ => { throw new Exception("Test"); };
            var sut = CreateSut(cameraService: cameraService);
            sut.EditingDocument = new Document();

            sut.AddPhotoFromCameraCommand.Execute(null);
            WaitForCommand();
            sut.EditingDocument.SubDocuments.Should().BeEmpty();
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileAddingPhotoFromCamera()
        {
            var tcs = new TaskCompletionSource<Photo>();
            var cameraService = new CameraServiceMock();
            cameraService.GetPhotoForDocumentFunc = _ => tcs.Task;
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
