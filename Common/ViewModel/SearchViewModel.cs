using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace MyDocs.Common.ViewModel
{
    public class SearchViewModel : ReactiveObject, ICanBeBusy, IDisposable
    {
        private readonly IDocumentService documentService;
        private readonly INavigationService navigationService;
        private readonly ITranslatorService translatorService;

        private IDisposable disposables;

        private readonly Tuple<int?, string> allYears = Tuple.Create<int?, string>(null, "All");
        private readonly ObservableAsPropertyHelper<IImmutableList<string>> categoryNames;
        private string queryText = string.Empty;
        private Tuple<int?, string> filterYear;
        private readonly ObservableAsPropertyHelper<IImmutableList<Tuple<int?, string>>> filterYears;
        private readonly ObservableAsPropertyHelper<IImmutableList<Filter>> filters;
        private bool isInDefaultLayout;
        private Filter allFilter;

        public IImmutableList<string> CategoryNames
        {
            get { return categoryNames.Value; }
        }

        public string QueryText
        {
            get { return queryText; }
            set { this.RaiseAndSetIfChanged(ref queryText, value); }
        }

        public IImmutableList<Filter> Filters
        {
            get { return filters.Value; }
        }

        private Filter activeFilter;
        public Filter ActiveFilter
        {
            get { return activeFilter; }
            set { this.RaiseAndSetIfChanged(ref activeFilter, value); }
        }

        public IImmutableList<Tuple<int?, string>> FilterYears
        {
            get { return filterYears.Value; }
        }

        public Tuple<int?, string> FilterYear
        {
            get { return filterYear; }
            set { this.RaiseAndSetIfChanged(ref filterYear, value); }
        }

        private readonly ObservableAsPropertyHelper<IImmutableList<Document>> results;
        public IImmutableList<Document> Results
        {
            get { return results.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> showFilters;
        public bool ShowFilters
        {
            get { return showFilters.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> hasResults;
        public bool HasResults
        {
            get { return hasResults.Value; }
        }

        public bool IsInDefaultLayout
        {
            get { return isInDefaultLayout; }
            set { this.RaiseAndSetIfChanged(ref isInDefaultLayout, value); }
        }

        private readonly ObservableAsPropertyHelper<bool> showDefaultResults;
        public bool ShowDefaultResults
        {
            get { return showDefaultResults.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> showTightResults;
        public bool ShowTightResults
        {
            get { return HasResults && !IsInDefaultLayout; }
        }

        public bool ShowNoResultsText { get { return !HasResults; } }

        public SearchViewModel(
            IDocumentService documentService,
            INavigationService navigationService,
            ITranslatorService translatorService)
        {
            this.documentService = documentService;
            this.navigationService = navigationService;
            this.translatorService = translatorService;

            categoryNames = documentService.GetCategoryNames()
                .Select(x => x.ToImmutableList())
                .ToProperty(this, x => x.CategoryNames);

            filterYears = documentService.GetDistinctDocumentYears()
                .Select(years => years.Select(y => Tuple.Create<int?, string>(y, y.ToString())))
                .Select(years => new[] { allYears }.Concat(years))
                .Select(years => years.ToImmutableList())
                .ToProperty(this, x => x.FilterYears, ImmutableList<Tuple<int?, string>>.Empty);

            this.WhenAnyValue(x => x.FilterYears)
                .TakeWhile(_ => FilterYear == null)
                .Subscribe(x => FilterYear = x.FirstOrDefault());

            ActiveFilter = allFilter = new Filter(translatorService.Translate("all"), _ => true);
            filters = documentService.GetCategoryNames()
                .Select(names => names.Select(name => new Filter(name, d => d.Category == name)))
                .Select(names => new[] { allFilter }.Concat(names))
                .Select(names => names.ToImmutableList())
                .ToProperty(this, x => x.Filters, ImmutableList<Filter>.Empty);

            showDefaultResults = this.WhenAnyValue(x => x.HasResults, x => x.IsInDefaultLayout, (x, y) => x && y)
                .ToProperty(this, x => x.ShowDefaultResults);
            showTightResults = this.WhenAnyValue(x => x.HasResults, x => x.IsInDefaultLayout, (x, y) => x && !y)
                .ToProperty(this, x => x.ShowTightResults);

            showFilters = this.WhenAnyValue(x => x.Filters)
                .Select(x => x.Any())
                .ToProperty(this, x => x.ShowFilters);

            var applyFilterSubscription = Observable.CombineLatest(
                documentService.GetDocuments(),
                this.WhenAnyValue(x => x.QueryText),
                this.WhenAnyValue(x => x.FilterYear).Select(x => x.Item1),
                this.WhenAnyValue(x => x.ActiveFilter),
                (docs, queryText, year, activeFilter) => new { docs, queryText, year, activeFilter }
            ).Subscribe(x => ApplyFilters(x.docs, x.queryText, x.year, x.activeFilter));

            results = this.WhenAnyValue(x => x.ActiveFilter.FilteredDocuments)
                .ToProperty(this, x => x.Results);

            hasResults = this.WhenAnyValue(x => x.Results)
                .Select(results => results.Any())
                .ToProperty(this, x => x.HasResults);

            disposables = new CompositeDisposable(
                categoryNames,
                filterYears,
                filters,
                showDefaultResults,
                showTightResults,
                showFilters,
                applyFilterSubscription,
                results,
                hasResults);

            CreateCommands();
            CreateDesignTimeData();
        }

        private void ApplyFilters(IEnumerable<Model.Logic.Document> docs, string text, int? year, Filter activeFilter)
        {
            var searchWords = QueryText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim());

            var preFilteredDocs =
                (from document in docs
                where searchWords.All(word =>
                    document.Tags.Any(t =>
                        t.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) >= 0
                    )
                )
                where year == null || year == document.DateAdded.Year
                select Document.FromLogic(document)).ToList();

            foreach (var filter in Filters)
            {
                filter.Apply(preFilteredDocs);
            }
        }

        #region Commands

        public ICommand ShowDocumentCommand { get; private set; }

        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        private void CreateCommands()
        {
            ShowDocumentCommand = this.CreateCommand(x => ShowDocument((Document)x));
        }

        private void ShowDocument(Document doc)
        {
            navigationService.Navigate<IShowDocumentPage>(doc.Id);
        }

        #endregion

        [Conditional("DEBUG")]
        private void CreateDesignTimeData()
        {
            if (ModeDetector.InDesignMode())
            {
                QueryText = "Tag 1";
            }
        }

        public void SetActiveFilter(Filter filter)
        {
            ActiveFilter = filter;
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        public class Filter : ReactiveObject
        {
            private string name;
            private Func<Document, bool> applies;
            private IImmutableList<Document> filteredDocuments = ImmutableList<Document>.Empty;

            public Filter(string name, Func<Document, bool> applies)
            {
                this.name = name;
                this.applies = applies;

                description = this.WhenAnyValue(x => x.Name, x => x.FilteredDocuments, (n, docs) => string.Format("{0} ({1})", n, docs.Count()))
                    .ToProperty(this, x => x.Description);
            }

            public void Apply(IEnumerable<Document> documents)
            {
                FilteredDocuments = documents
                    .Where(d => applies(d))
                    .ToImmutableList();
            }

            public string Name
            {
                get { return name; }
                set { this.RaiseAndSetIfChanged(ref name, value); }
            }

            public IImmutableList<Document> FilteredDocuments
            {
                get { return filteredDocuments; }
                set { this.RaiseAndSetIfChanged(ref filteredDocuments, value); }
            }

            private readonly ObservableAsPropertyHelper<string> description;
            public string Description
            {
                get { return description.Value; }
            }
        }
    }
}
