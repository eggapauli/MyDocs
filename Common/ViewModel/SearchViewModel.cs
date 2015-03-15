using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyDocs.Common.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private IDocumentService documentService;
        private INavigationService navigationService;
        private ITranslatorService translatorService;

        private string queryText;
        private Tuple<int?, string> filterYear;
        private List<int> filterYears;
        private List<Filter> filters;
        private bool isInDefaultLayout;
        private Filter allFilter;

        public IEnumerable<string> CategoryNames
        {
            get { return documentService.GetCategoryNames(); }
        }

        public string QueryText
        {
            get { return queryText; }
            set { Set(ref queryText, value); }
        }

        public IEnumerable<Filter> Filters
        {
            get
            {
                yield return allFilter;
                foreach (var filter in filters) {
                    yield return filter;
                }
            }
        }

        private Tuple<int?, string> allYears = Tuple.Create<int?, string>(null, "All");
        public IEnumerable<Tuple<int?, string>> FilterYears
        {
            get
            {
                yield return allYears;
                foreach (var year in documentService.GetDistinctDocumentYears()) {
                    yield return Tuple.Create<int?, string>(year, year.ToString());
                }
            }
        }

        public Tuple<int?, string> FilterYear
        {
            get { return filterYear ?? FilterYears.First(); }
            set { Set(ref filterYear, value); }
        }

        public IEnumerable<Document> Results
        {
            get { return Filters.Single(f => f.Active).FilteredDocuments; }
        }

        public bool ShowFilters
        {
            get { return filters.Count > 0; }
        }

        public bool HasResults
        {
            get { return Filters.Single(f => f.Active).FilteredDocuments.Any(); }
        }

        public bool IsInDefaultLayout
        {
            get { return isInDefaultLayout; }
            set
            {
                if (Set(ref isInDefaultLayout, value)) {
                    RaisePropertyChanged(() => ShowDefaultResults);
                    RaisePropertyChanged(() => ShowTightResults);
                }
            }
        }

        public bool ShowDefaultResults { get { return HasResults && IsInDefaultLayout; } }

        public bool ShowTightResults { get { return HasResults && !IsInDefaultLayout; } }

        public bool ShowNoResultsText { get { return !HasResults; } }

        public SearchViewModel(
            IDocumentService documentService,
            INavigationService navigationService,
            ITranslatorService translatorService)
        {
            this.documentService = documentService;
            this.navigationService = navigationService;
            this.translatorService = translatorService;

            QueryText = string.Empty;
            filters = new List<Filter>();
            filterYears = new List<int>();
            allFilter = new Filter(translatorService.Translate("all"), _ => true, active: true);

            this.PropertyChanged += async (s, e) => {
                var propertyNames = new[] { "FilterYear" };
                foreach (var name in propertyNames) {
                    VerifyPropertyName(name);
                }

                if (propertyNames.Contains(e.PropertyName)) {
                    await RefreshResults();
                }
            };

            CreateCommands();
            CreateDesignTimeData();
        }

        #region Commands

        public RelayCommand<Document> ShowDocumentCommand { get; private set; }

        private void CreateCommands()
        {
            ShowDocumentCommand = new RelayCommand<Document>(ShowDocument);
        }

        private void ShowDocument(Document doc)
        {
            navigationService.Navigate<IShowDocumentPage>(doc.Id);
        }

        #endregion

        [Conditional("DEBUG")]
        private async void CreateDesignTimeData()
        {
            if (IsInDesignMode) {
                QueryText = "Tag 1";
                await RefreshResults();
            }
        }

        public void LoadFilters()
        {
            filters.Clear();
            var newFilters = documentService.GetCategoryNames()
                .Select(categoryName =>
                    new SearchViewModel.Filter(categoryName, d => d.Category == categoryName));
            filters.AddRange(newFilters);

            RaisePropertyChanged(() => Filters);
            RaisePropertyChanged(() => ShowFilters);
            RaisePropertyChanged(() => HasResults);
            RaisePropertyChanged(() => Results);
        }

        public async Task RefreshResults()
        {
            var searchWords = QueryText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim());

            await documentService.LoadAsync();

            var docs = (from document in await documentService.LoadAsync()
                        where searchWords.All(word =>
                            document.Tags.Any(t =>
                                t.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) >= 0
                            )
                        )
                        where !FilterYear.Item1.HasValue || document.DateAdded.Year == FilterYear.Item1.Value
                        select Document.FromLogic(document)).ToList();
            foreach (var filter in Filters) {
                filter.Apply(docs);
            }

            RaisePropertyChanged(() => Results);
            RaisePropertyChanged(() => HasResults);
            RaisePropertyChanged(() => ShowDefaultResults);
            RaisePropertyChanged(() => ShowTightResults);
            RaisePropertyChanged(() => ShowNoResultsText);
        }

        public class Filter : ObservableObject
        {
            private string name;
            private Func<Document, bool> applies;
            private IEnumerable<Document> filteredDocuments;
            private bool active;

            public Filter(string name, Func<Document, bool> applies, bool active = false)
            {
                this.name = name;
                this.applies = applies;
                this.active = active;

                this.PropertyChanged += (s, e) => {
                    var propertyNames = new[] { "Name", "FilteredDocuments" };
                    foreach (var propertyName in propertyNames) {
                        VerifyPropertyName(propertyName);
                    }

                    if (propertyNames.Contains(e.PropertyName)) {
                        RaisePropertyChanged(() => Description);
                    }
                };
            }

            public void Apply(IEnumerable<Document> documents)
            {
                FilteredDocuments = documents.Where(d => applies(d));
            }

            public string Name
            {
                get { return name; }
                set { Set(ref name, value); }
            }

            public IEnumerable<Document> FilteredDocuments
            {
                get { return filteredDocuments ?? Enumerable.Empty<Document>(); }
                set { Set(ref filteredDocuments, value); }
            }

            public bool Active
            {
                get { return active; }
                set { Set(ref active, value); }
            }

            public string Description
            {
                get { return string.Format("{0} ({1})", name, FilteredDocuments.Count()); }
            }
        }

        public async Task SetActiveFilter(Filter filter)
        {
            foreach (var activeFilter in Filters.Where(f => f.Active)) {
                activeFilter.Active = false;
            }

            filter = filter ?? Filters.First();
            filter.Active = true;

            await RefreshResults();
        }
    }
}
