using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
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
        public void DeletedDocumentShouldBeRemovedFromList()
        {
            var documentService = CreateDocumentServiceMock();
            var sut = CreateSut(documentService: documentService);
            var docs = sut.Categories.SelectMany(c => c.Documents);
            var doc = docs.OrderBy(_ => Guid.NewGuid()).First();
            sut.SelectedDocument = doc;

            sut.DeleteDocumentCommand.Execute(null);
            WaitForCommand();
            docs.Should().NotContain(doc);
        }

        [TestMethod]
        public void DeletedDocumentShouldCallServiceMethod()
        {
            var documentService = CreateDocumentServiceMock();
            var deletedDocumentIds = new List<Guid>();
            documentService.DeleteDocumentAsyncFunc = async d =>
            {
                deletedDocumentIds.Add(d.Id);
                await Task.Yield();
            };
            var sut = CreateSut(documentService: documentService);
            var doc = sut.Categories
                .SelectMany(c => c.Documents)
                .OrderBy(_ => Guid.NewGuid())
                .First();
            sut.SelectedDocument = doc;

            sut.DeleteDocumentCommand.Execute(null);
            WaitForCommand();
            deletedDocumentIds.Should().BeEquivalentTo(doc.Id);
        }

        [TestMethod]
        public void DocumentShouldBeUnselectedWhenDeleted()
        {
            var documentService = CreateDocumentServiceMock();
            var sut = CreateSut(documentService: documentService);
            sut.SelectedDocument = new Model.View.Document();

            sut.DeleteDocumentCommand.Execute(null);
            WaitForCommand();
            sut.SelectedDocument.Should().BeNull();
        }

        [TestMethod]
        public void ViewModelShouldBeBusyWhileDeletingDocuments()
        {
            var tcs = new TaskCompletionSource<object>();
            var documentService = CreateDocumentServiceMock();
            documentService.DeleteDocumentAsyncFunc = delegate { return tcs.Task; };
            var sut = CreateSut(documentService: documentService);
            sut.SelectedDocument = new Model.View.Document();

            sut.IsBusy.Should().BeFalse();
            sut.DeleteDocumentCommand.Execute(null);
            WaitForCommand();
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            WaitForCommand();
            sut.IsBusy.Should().BeFalse();
        }

        private Document CreateTestDocument()
        {
            return new Document(Guid.NewGuid(), "Testcategory", DateTime.Now, TimeSpan.FromDays(2 * 365), true, new[] { "tag1", "tag2" });
        }
    }
}
