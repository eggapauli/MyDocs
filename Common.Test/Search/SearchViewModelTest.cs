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
using System.Collections.Immutable;

namespace MyDocs.Common.Test.Search
{
    [TestClass]
    public class SearchViewModelTest
    {
        private List<Category> categories;
        private IDocumentService documentService;

        [TestInitialize]
        public void Init()
        {
            categories = MakeTestDocuments().ToList();
            documentService = MakeDocumentService(categories);
        }

        [TestMethod]
        public async Task RefreshResults_SingleTag_ReturnsDocumentsWithTag()
        {
            var expected = categories.First().Documents.Take(2).ToList();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag2";

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        [TestMethod]
        public async Task RefreshResults_MultipleTags_ReturnsDocumentsWhichHaveAllTags()
        {
            var expected = categories.First().Documents.Take(2).ToList();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag2 tag3";

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        [TestMethod]
        public async Task RefreshResults_Always_ReturnsDistinctResults()
        {
            var expected = categories.First().Documents.First();
            var sut = MakeSut(documentService);
            sut.QueryText = "tag1 tag2";

            await sut.RefreshResults();

            Assert.AreEqual(1, sut.Results.Count(r => r.Equals(expected)));
        }

        [TestMethod]
        public async Task RefreshResults_YearFilter_ReturnsCorrectResults()
        {
            var expected = categories.First().Documents.Skip(1).ToList();
            var sut = MakeSut(documentService);
            sut.FilterYear = sut.FilterYears.First(filterYear => filterYear.Item1.HasValue && filterYear.Item1 == 2014);

            await sut.RefreshResults();

            CollectionAssert.AreEqual(expected, sut.Results.ToList());
        }

        private IEnumerable<Category> MakeTestDocuments()
        {
            yield return new Category("testcategory", new[] {
                new Document(Guid.NewGuid(), "testcategory", new DateTime(2013, 12, 31, 8, 0, 0), TimeSpan.FromHours(5), true, new[] { "tag1", "tag2", "tag3" }),
                new Document(Guid.NewGuid(), "testcategory", new DateTime(2014, 1, 1, 9, 0, 0), TimeSpan.FromHours(1), true, new[] { "tag2", "tag3", "tag4" }),
                new Document(Guid.NewGuid(), "testcategory", new DateTime(2014, 1, 2, 10, 0, 0), TimeSpan.FromHours(2), true, new[] { "tag3", "tag4", "tag5" })
            });
        }

        private IDocumentService MakeDocumentService(IEnumerable<Category> categories)
        {
            var documentService = A.Fake<IDocumentService>();
            A.CallTo(() => documentService.LoadAsync()).Returns(Task.FromResult(categories.ToImmutableList()));
            A.CallTo(() => documentService.GetCategoryNames()).Returns(categories.Select(c => c.Name));
            var years = categories
                .SelectMany(c => c.Documents)
                .Select(d => d.DateAdded.Year)
                .Distinct()
                .OrderBy(year => year);
            A.CallTo(() => documentService.GetDistinctDocumentYears()).Returns(years);
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
