using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Model.View;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class EditDocumentViewModelTest
    {
        [TestMethod]
        public void RemovePhotoCommandShouldBeDisabledWhenNoPhotoIsSelected()
        {
            var sut = CreateSut();
            sut.EditingDocument = new Document();
            sut.SelectedPhoto = null;

            sut.RemovePhotoCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public async Task RemovePhotoCommandShouldBeEnabledWhenPhotoIsSelected()
        {
            var sut = CreateSut();
            sut.EditingDocument = new Document();
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png"));
            sut.SelectedPhoto = new Photo(file);

            sut.RemovePhotoCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public async Task ShouldDeselectRemovedPhoto()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png"));
            var photo = new Photo(file);
            var sut = CreateSut();
            sut.EditingDocument = new Document();
            sut.EditingDocument.AddSubDocument(new SubDocument(file, Enumerable.Repeat(photo, 1)));
            sut.SelectedPhoto = photo;

            sut.RemovePhotoCommand.Execute(null);
            WaitForCommand();

            sut.SelectedPhoto.Should().BeNull();
        }

        [TestMethod]
        public async Task ShouldRemovePhotoFromSubDocument()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png"));
            var photo = new Photo(file);
            var sut = CreateSut();
            sut.EditingDocument = new Document();
            sut.EditingDocument.AddSubDocument(new SubDocument(file, Enumerable.Repeat(photo, 1)));
            sut.SelectedPhoto = photo;

            sut.RemovePhotoCommand.Execute(null);
            WaitForCommand();

            sut.EditingDocument.SubDocuments.SelectMany(d => d.Photos).Should().NotContain(photo);
        }

        [TestMethod]
        public async Task ShouldRemoveSubDocumentIfEmpty()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png"));
            var photo = new Photo(file);
            var sut = CreateSut();
            sut.EditingDocument = new Document();
            var subDocument = new SubDocument(file, Enumerable.Repeat(photo, 1));
            sut.EditingDocument.AddSubDocument(subDocument);
            sut.SelectedPhoto = photo;

            sut.RemovePhotoCommand.Execute(null);
            WaitForCommand();

            sut.EditingDocument.SubDocuments.Should().NotContain(subDocument);
        }
    }
}
