using Common.Test.Mocks;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MyDocs.Common.Contract.Page;
using System;
using System.Collections.Generic;

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
            var navigatedToTypes = new List<Tuple<Type, object>>();
            var navigationService = new NavigationServiceMock();
            navigationService.NavigateAction = (t, o) => navigatedToTypes.Add(Tuple.Create(t, o));
            var sut = CreateSut(navigationService: navigationService);

            sut.AddDocumentCommand.Execute(null);
            navigatedToTypes.Should().BeEquivalentTo(Tuple.Create(typeof(IEditDocumentPage), (object)null));
        }
    }
}
