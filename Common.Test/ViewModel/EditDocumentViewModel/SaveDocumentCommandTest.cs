using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Model.View;
using MyDocs.Common.ViewModel;
using System;
using System.Collections.Immutable;
using System.Linq;
using Windows.Storage;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class EditDocumentViewModelTest
    {
        [TestMethod]
        public void SaveDocumentCommandShouldBeDisabledWhenEditingDocumentIsNull()
        {
            var sut = CreateSutForSavingDocuments();
            sut.EditingDocument = null;

            sut.SaveDocumentCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void SaveDocumentCommandShouldBeDisabledWhenTagsAreEmpty()
        {
            var sut = CreateSutForSavingDocuments();
            sut.EditingDocument.Tags = ImmutableList<string>.Empty;

            sut.SaveDocumentCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void SaveDocumentCommandShouldBeDisabledWhenNewCategoryNameIsEmpty()
        {
            var sut = CreateSutForSavingDocuments();
            sut.ShowNewCategoryInput = true;
            sut.NewCategoryName = "";

            sut.SaveDocumentCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void SaveDocumentCommandShouldBeDisabledWhenUseCategoryNameIsEmpty()
        {
            var sut = CreateSutForSavingDocuments();
            sut.ShowUseCategoryInput = true;
            sut.UseCategoryName = "";

            sut.SaveDocumentCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void SaveDocumentCommandShouldBeDisabledWhenDocumentDoesntHaveSubDocuments()
        {
            var sut = CreateSutForSavingDocuments();
            sut.EditingDocument.SubDocuments = ImmutableList<SubDocument>.Empty;

            sut.SaveDocumentCommand.CanExecute(null).Should().BeFalse();
        }

        private EditDocumentViewModel CreateSutForSavingDocuments()
        {
            var sut = CreateSut();
            sut.EditingDocument = CreateSampleDocument();
            sut.ShowNewCategoryInput = true;
            sut.NewCategoryName = "newcat";
            sut.CategoryNames = new[] { "cat1", "cat2" };
            sut.UseCategoryName = sut.CategoryNames.First();
            return sut;
        }

        private Document CreateSampleDocument()
        {
            var file = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Images/UnitTestLogo.scale-100.png")).AsTask().Result;
            var subDocument = new SubDocument(file, new[] { new Photo(file) });
            return new Document(Guid.NewGuid(), "cat", DateTime.Now, TimeSpan.FromDays(1), true, new[] { "tag1", "tag2" }, new[] { subDocument });
        }
    }
}
