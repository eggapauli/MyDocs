using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.ViewModel;
using System.Threading.Tasks;

namespace MyDocs.Common.Test.ViewModel
{
    [TestClass]
    public class DocumentViewModelTest
    {
        [TestMethod]
        public void AddDocumentCommandShouldBeEnabled()
        {
            var sut = CreateSut();

            sut.AddDocumentCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void AddDocumentCommandShouldNavigateToEditPage()
        {
            var navigationService = A.Fake<INavigationService>();
            var sut = CreateSut(navigationService: navigationService);

            using (Fake.CreateScope())
            {
                sut.AddDocumentCommand.Execute(null);
                A.CallTo(() => navigationService.Navigate<IEditDocumentPage>()).MustHaveHappened();
            }
        }

        [TestMethod]
        public void EditDocumentCommandShouldBeDisabledWhenNoDocumentIsSelected()
        {
            var sut = CreateSut();
            sut.SelectedDocument = null;

            sut.EditDocumentCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void EditDocumentCommandShouldBeEnabledWhenDocumentIsSelected()
        {
            var sut = CreateSut();
            sut.SelectedDocument = new Model.View.Document();

            sut.EditDocumentCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void EditDocumentCommandShouldNavigateToEditPage()
        {
            var navigationService = A.Fake<INavigationService>();
            var sut = CreateSut(navigationService: navigationService);
            sut.SelectedDocument = new Model.View.Document();

            using (Fake.CreateScope())
            {
                sut.EditDocumentCommand.Execute(null);
                A.CallTo(() => navigationService.Navigate<IEditDocumentPage>(sut.SelectedDocument.Id)).MustHaveHappened();
            }
        }

        [TestMethod]
        public void DeleteDocumentCommandShouldBeDisabledWhenNoDocumentIsSelected()
        {
            var sut = CreateSut();
            sut.SelectedDocument = null;

            sut.DeleteDocumentCommand.CanExecute(null).Should().BeFalse();
        }

        [TestMethod]
        public void DeleteDocumentCommandShouldBeEnabledWhenDocumentIsSelected()
        {
            var sut = CreateSut();
            sut.SelectedDocument = new Model.View.Document();

            sut.DeleteDocumentCommand.CanExecute(null).Should().BeTrue();
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileDeletingDocuments()
        {
            var tcs = new TaskCompletionSource<object>();
            var documentService = A.Fake<IDocumentService>();
            A.CallTo(() => documentService.DeleteDocumentAsync(A<Model.Logic.Document>._)).Returns(tcs.Task);
            var sut = CreateSut(documentService: documentService);
            sut.SelectedDocument = new Model.View.Document();
            
            sut.IsBusy.Should().BeFalse();
            sut.DeleteDocumentCommand.Execute(null);
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            sut.IsBusy.Should().BeFalse();
        }

        [TestMethod]
        public void RenamingCategoryShouldBeDisabledWhenCategoryNameIsEmpty()
        {
            var sut = CreateSut();
            sut.NewCategoryName = "";

            sut.RenameCategoryCommand.CanExecute(null).Should().BeFalse();
        }

        private static DocumentViewModel CreateSut(
            IDocumentService documentService = null,
            IUserInterfaceService uiService = null,
            INavigationService navigationService = null,
            ILicenseService licenseService = null,
            IExportDocumentService exportDocumentService = null,
            IImportDocumentService importDocumentService = null)
        {
            documentService = documentService ?? A.Fake<IDocumentService>();
            uiService = uiService ?? A.Fake<IUserInterfaceService>();
            navigationService = navigationService ?? A.Fake<INavigationService>();
            licenseService = licenseService ?? A.Fake<ILicenseService>();
            exportDocumentService = exportDocumentService ?? A.Fake<IExportDocumentService>();
            importDocumentService = importDocumentService ?? A.Fake<IImportDocumentService>();
            return new DocumentViewModel(documentService, uiService, navigationService, licenseService, exportDocumentService, importDocumentService);
        }
    }
}
