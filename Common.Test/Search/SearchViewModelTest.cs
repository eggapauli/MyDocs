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
            var expected = documents.Take(2).ToList();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag2";

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        [TestMethod]
        public async Task RefreshResults_MultipleTags_ReturnsDocumentsWhichHaveAllTags()
        {
            var expected = documents.Take(2).ToList();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag2 tag3";

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        [TestMethod]
        public async Task RefreshResults_Always_ReturnsDistinctResults()
        {
            var expected = documents.First();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag1 tag2";

            await sut.RefreshResults();

            Assert.AreEqual(1, sut.Results.Count(r => r.Equals(expected)));
        }

        [TestMethod]
        public async Task RefreshResults_YearFilter_ReturnsCorrectResults()
        {
            var expected = documents.Skip(1).ToList();
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
            var documentService = A.Fake<IDocumentService>();
            A.CallTo(() => documentService.Documents).Returns(new ObservableCollection<Document>(documents));
            A.CallTo(() => documentService.GetCategoryNames()).Returns(documents.Select(d => d.Category).Distinct());
            A.CallTo(() => documentService.GetDistinctDocumentYears()).Returns(documents.Select(d => d.DateAdded.Year).Distinct().OrderBy(year => year));
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
