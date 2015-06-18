using Common.Test.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class DocumentViewModelTest
    {
        [TestMethod]
        public void DeleteCategoryCommandShouldBeEnabled()
        {
            var sut = CreateSut();

            sut.DeleteCategoryCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void ViewModelShouldCallServiceMethodWhenDeletingCategory()
        {
            const string oldCategoryName = "Old category";
            const string newCategoryName = "New category";

            var renamedCategories = new List<Tuple<string, string>>();
            var documentService = CreateDocumentServiceMock();
            var sut = CreateSut(documentService: documentService);
            sut.NewCategoryName = newCategoryName;

            sut.DeleteCategoryCommand.Execute(new Model.View.Category(oldCategoryName));
            WaitForCommand();
            sut.Categories.Where(c => c.Name == oldCategoryName).Should().BeEmpty();
            sut.Categories.SelectMany(c => c.Documents).Where(d => d.Category == oldCategoryName).Should().BeEmpty();
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileDeletingCategories()
        {
            var tcs = new TaskCompletionSource<object>();
            var documentService = CreateDocumentServiceMock();
            documentService.DeleteCategoryAsyncFunc = delegate { return tcs.Task; };
            var sut = CreateSut(documentService: documentService);
            sut.NewCategoryName = "New category";

            sut.IsBusy.Should().BeFalse();
            sut.DeleteCategoryCommand.Execute(new Model.View.Category("Old category"));
            WaitForCommand();
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            WaitForCommand();
            sut.IsBusy.Should().BeFalse();
        }
    }
}
