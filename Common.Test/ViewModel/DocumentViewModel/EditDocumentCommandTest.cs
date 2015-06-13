using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;

namespace MyDocs.Common.Test.ViewModel
{
    public partial class DocumentViewModelTest
    {
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
    }
    }
