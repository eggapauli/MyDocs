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
            var navigationService = new NavigationServiceMock();
            var navigatedTo = new List<Tuple<Type, object>>();
            navigationService.NavigateAction = (t, o) => navigatedTo.Add(Tuple.Create(t, o));
            var sut = CreateSut(navigationService: navigationService);
            var doc = new Model.View.Document();
            sut.SelectedDocument = doc;

            sut.EditDocumentCommand.Execute(null);
            navigatedTo.Should().BeEquivalentTo(Tuple.Create(typeof(IEditDocumentPage), (object)doc.Id));
        }
    }
}
