using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyDocs.Common.ViewModel;
using FakeItEasy;
using MyDocs.Common.Contract.Service;
using System.Collections.Generic;
using MyDocs.Common.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Test.Search
{
    [TestClass]
    public class SearchViewModelTest
    {
        [TestMethod]
        public async Task RefreshResults_SpaceDelimitedStrings_Succeeds()
        {
            var documents = MakeTestDocuments().ToList();
            var expected = documents.First();
            var documentService = MakeDocumentService(documents);
            var sut = MakeSut(documentService);
            sut.QueryText = "tag1 tag2";

            await sut.RefreshResults();

            Assert.IsTrue(sut.Results.Any(r => r.Equals(expected)));
        }

        [TestMethod]
        public async Task RefreshResults_Always_ReturnsDistinctResults()
        {
            var documents = MakeTestDocuments().ToList();
            var expected = documents.First();
            var documentService = MakeDocumentService(documents);
            var sut = MakeSut(documentService);
            sut.QueryText = "tag1,tag2";

            await sut.RefreshResults();

            Assert.AreEqual(1, sut.Results.Count(r => r.Equals(expected)));
        }

        private IEnumerable<Document> MakeTestDocuments()
        {
            yield return new Document(Guid.NewGuid(), "testcategory", DateTime.Now, TimeSpan.FromDays(5), true, new[] { "tag1", "tag2", "tag3" });
            yield return new Document(Guid.NewGuid(), "testcategory", DateTime.Now, TimeSpan.FromDays(5), true, new[] { "tag4", "tag5", "tag6" });
            yield return new Document(Guid.NewGuid(), "testcategory", DateTime.Now, TimeSpan.FromDays(5), true, new[] { "tag7", "tag8", "tag9" });
        }

        private IDocumentService MakeDocumentService(IEnumerable<Document> documents)
        {
            var documentService = A.Fake<IDocumentService>();
            A.CallTo(() => documentService.Documents).Returns(new ObservableCollection<Document>(documents));
            return documentService;
        }

        private SearchViewModel MakeSut(IDocumentService documentService)
        {
            var navigationService = A.Fake<INavigationService>();
            var translatorService = A.Fake<ITranslatorService>();
            var sut = new SearchViewModel(documentService, navigationService, translatorService);
            sut.LoadFilters();
            return sut;
        }
    }
}
