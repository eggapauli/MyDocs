using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Immutable;
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
        public void ViewModelShouldCallServiceMethodWhenDeletingDocuments()
        {
            var documentService = A.Fake<IDocumentService>();
            var sut = CreateSut(documentService: documentService);
            sut.SelectedDocument = new Model.View.Document();

            using (Fake.CreateScope())
            {
                sut.DeleteDocumentCommand.Execute(null);
                WaitForCommand();
                A.CallTo(() => documentService.DeleteDocumentAsync(A<Model.Logic.Document>._)).MustHaveHappened();
            }
        }

        [TestMethod]
        public void DocumentShouldBeUnselectedWhenDeleted()
        {
            var documentService = A.Fake<IDocumentService>();
            var sut = CreateSut(documentService: documentService);
            sut.SelectedDocument = new Model.View.Document();

            sut.DeleteDocumentCommand.Execute(null);
            WaitForCommand();
            sut.SelectedDocument.Should().BeNull();
        }

        [TestMethod]
        public void DocumentShouldBeRemovedFromListWhenDeleted()
        {
            var documentService = A.Fake<IDocumentService>();
            var doc = CreateTestDocument();
            A.CallTo(() => documentService.LoadAsync()).Returns(ImmutableList<Model.Logic.Document>.Empty.Add(doc));
            var sut = CreateSut(documentService: documentService);
            sut.LoadAsync().Wait();
            sut.SelectedDocument = sut.Categories.First().Documents.First();

            sut.DeleteDocumentCommand.Execute(null);
            WaitForCommand();
            sut.Categories.SelectMany(c => c.Documents).Select(d => d.Id).Should().NotContain(doc.Id);
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
            WaitForCommand();
            sut.IsBusy.Should().BeTrue();
            tcs.SetResult(null);
            WaitForCommand();
            sut.IsBusy.Should().BeFalse();
        }

        private Model.Logic.Document CreateTestDocument()
        {
            return new Model.Logic.Document(Guid.NewGuid(), "Testcategory", DateTime.Now, TimeSpan.FromDays(2 * 365), true, new[] { "tag1", "tag2" });
        }
    }
}
