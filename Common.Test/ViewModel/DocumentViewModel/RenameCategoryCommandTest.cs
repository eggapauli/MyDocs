using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
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

            var documentService = CreateDocumentServiceMock();
            var renamedCategories = new List<Tuple<string, string>>();
            documentService.RenameCategoryAsyncFunc = async (x, y) =>
            {
                renamedCategories.Add(Tuple.Create(x, y));
                await Task.Yield();
            };
            var sut = CreateSut(documentService: documentService);
            sut.NewCategoryName = newCategoryName;

            sut.RenameCategoryCommand.Execute(new Model.View.Category(oldCategoryName));
            renamedCategories.Should().BeEquivalentTo(Tuple.Create(oldCategoryName, newCategoryName));
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileRenamingCategories()
        {
            var tcs = new TaskCompletionSource<object>();
            var documentService = CreateDocumentServiceMock();
            documentService.RenameCategoryAsyncFunc = delegate { return tcs.Task; };
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
            var documentService = CreateDocumentServiceMock();
            var sut = CreateSut(documentService: documentService);
            sut.NewCategoryName = "New category";

            sut.RenameCategoryCommand.Execute(new Model.View.Category("Old category"));
            WaitForCommand();
            sut.NewCategoryName.Should().BeNullOrEmpty();
        }
    }
}
