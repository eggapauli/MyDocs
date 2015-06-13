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
    }
}
