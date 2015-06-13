using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
using System.Threading.Tasks;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class DocumentViewModelTest
    {
        [TestMethod]
        public void RenamingCategoryShouldBeDisabledWhenCategoryNameIsEmpty()
        {
            var sut = CreateSut();
            sut.NewCategoryName = "";

            sut.RenameCategoryCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void RenamingCategoryCommandShouldBeEnabledWhenCategoryNameIsSet()
        {
            var sut = CreateSut();
            sut.NewCategoryName = "New category";

            sut.RenameCategoryCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void ViewModelShouldCallServiceMethodWhenRenamingCategory()
        {
            const string oldCategoryName = "Old category";
            const string newCategoryName = "New category";

            var documentService = A.Fake<IDocumentService>();
            var sut = CreateSut(documentService: documentService);
            sut.NewCategoryName = newCategoryName;

            using (Fake.CreateScope())
            {
                sut.RenameCategoryCommand.Execute(new Model.View.Category(oldCategoryName));
                A.CallTo(() => documentService.RenameCategoryAsync(oldCategoryName, newCategoryName)).MustHaveHappened();
            }
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileRenamingCategories()
        {
            var tcs = new TaskCompletionSource<object>();
            var documentService = A.Fake<IDocumentService>();
            A.CallTo(() => documentService.RenameCategoryAsync(A<string>._, A<string>._)).Returns(tcs.Task);
            var sut = CreateSut(documentService: documentService);
            sut.NewCategoryName = "New category";

            sut.IsBusy.Should().BeFalse();
            sut.RenameCategoryCommand.Execute(new Model.View.Category("Old category"));
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            sut.IsBusy.Should().BeFalse();
        }

        [TestMethod]
        public void NewCategoryNameShouldBeResetAfterRenamingCategories()
        {
            var documentService = A.Fake<IDocumentService>();
            var sut = CreateSut(documentService: documentService);
            sut.NewCategoryName = "New category";

            sut.RenameCategoryCommand.Execute(new Model.View.Category("Old category"));
            sut.NewCategoryName.Should().BeNullOrEmpty();
        }
    }
}
