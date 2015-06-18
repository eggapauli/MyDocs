using System;
using MyDocs.Common.ViewModel;
using MyDocs.Common.Contract.Service;
using System.Collections.Generic;
using MyDocs.Common.Model.Logic;
using View = MyDocs.Common.Model.View;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Common.Test.Mocks;

namespace MyDocs.Common.Test.Search
{
    [TestClass]
    public class SearchViewModelTest
    {
        private List<Document> documents;
        private IDocumentService documentService;

        [TestInitialize]
        public void Init()
        {
            documents = MakeTestDocuments().ToList();
            documentService = MakeDocumentService(documents);
        }

        [TestMethod]
        public async Task RefreshResults_SingleTag_ReturnsDocumentsWithTag()
        {
            var expected = documents.Take(2).Select(View.Document.FromLogic).ToList();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag2";

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        [TestMethod]
        public async Task RefreshResults_MultipleTags_ReturnsDocumentsWhichHaveAllTags()
        {
            var expected = documents.Take(2).Select(View.Document.FromLogic).ToList();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag2 tag3";

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        [TestMethod]
        public async Task RefreshResults_Always_ReturnsDistinctResults()
        {
            var expected = View.Document.FromLogic(documents.First());
            var sut = MakeSut(documentService);
            sut.QueryText = "tag1 tag2";

            await sut.RefreshResults();

            Assert.AreEqual(1, sut.Results.Count(r => r.Equals(expected)));
        }

        [TestMethod]
        public async Task RefreshResults_YearFilter_ReturnsCorrectResults()
        {
            var expected = documents.Skip(1).Select(View.Document.FromLogic).ToList();
            var sut = MakeSut(documentService);
            sut.FilterYear = sut.FilterYears.First(filterYear => filterYear.Item1.HasValue && filterYear.Item1 == 2014);

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        private IEnumerable<Document> MakeTestDocuments()
        {
            yield return new Document(Guid.NewGuid(), "testcategory", new DateTime(2013, 12, 31, 8, 0, 0), TimeSpan.FromHours(5), true, new[] { "tag1", "tag2", "tag3" });
            yield return new Document(Guid.NewGuid(), "testcategory", new DateTime(2014, 1, 1, 9, 0, 0), TimeSpan.FromHours(1), true, new[] { "tag2", "tag3", "tag4" });
            yield return new Document(Guid.NewGuid(), "testcategory", new DateTime(2014, 1, 2, 10, 0, 0), TimeSpan.FromHours(2), true, new[] { "tag3", "tag4", "tag5" });
        }

        private IDocumentService MakeDocumentService(IEnumerable<Document> documents)
        {
            return new DocumentServiceMock(documents);
        }

        private SearchViewModel MakeSut(IDocumentService documentService)
        {
            var navigationService = new NavigationServiceMock();
            var translatorService = new TranslatorServiceMock();
            var sut = new SearchViewModel(documentService, navigationService, translatorService);
            sut.LoadFilters();
            return sut;
        }
    }
}
