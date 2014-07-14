using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model;
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
        private IList<Filter> filters;
        private IEnumerable<Document> results;
        private bool isInDefaultLayout;

        public IEnumerable<string> CategoryNames
        {
            get { return documentService.GetCategoryNames(); }
        }

        public string QueryText
        {
            get { return queryText; }
            set { Set(ref queryText, value); }
        }

        public IList<Filter> Filters
        {
            get { return filters; }
            set { Set(ref filters, value); }
        }

        public IEnumerable<Document> Results
        {
            get { return results; }
            set
            {
                if (Set(ref results, value)) {
                    RaisePropertyChanged(() => HasResults);
                    RaisePropertyChanged(() => ShowDefaultResults);
                    RaisePropertyChanged(() => ShowTightResults);
                    RaisePropertyChanged(() => ShowNoResultsText);
                }
            }
        }

        public bool ShowFilters
        {
            get { return Filters != null && Filters.Count > 1; }
        }

        public bool HasResults
        {
            get { return Filters.Single(f => f.Active).Count > 0; }
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

            QueryText = "";
            
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
        private void CreateDesignTimeData()
        {
            if (IsInDesignMode) {
                QueryText = "Tag 1";
                Filters = new List<Filter> {
                    new Filter(translatorService.Translate("all"), active: true, isAll: true)
                };
                var t = RefreshResults();
            }
        }

        public void LoadFilters()
        {
            Filters = new ObservableCollection<Filter> {
                new Filter(translatorService.Translate("all"), active: true, isAll: true)
            };
            foreach (string categoryName in CategoryNames) {
                Filters.Add(new SearchViewModel.Filter(categoryName));
            }
        }

        public async Task RefreshResults()
        {
            var searchWords = QueryText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim());

            await documentService.LoadDocumentsAsync();

            var docs = (from document in documentService.Documents
                        where searchWords.All(word => 
                            document.Tags.Any(t =>
                                t.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) >= 0
                            )
                        )
                        select document).ToList();
            foreach (var filter in Filters) {
                IEnumerable<Document> results;
                if (filter.IsAll) {
                    filter.Count = docs.Count;
                    results = docs;
                }
                else {
                    results = docs.Where(d => d.Category == filter.Name);
                    filter.Count = results.Count();
                }
                if (filter.Active) {
                    Results = results;
                }
            }
        }

        public class Filter : ObservableObject
        {
            private string name;
            private int count;
            private bool active;
            private bool isAll;

            public Filter(string name, int count = 0, bool active = false, bool isAll = false)
            {
                Name = name;
                Count = count;
                Active = active;
                IsAll = isAll;
            }

            public override String ToString()
            {
                return Description;
            }

            public string Name
            {
                get { return name; }
                set
                {
                    if (Set(ref name, value)) {
                        RaisePropertyChanged(() => Description);
                    }
                }
            }

            public int Count
            {
                get { return count; }
                set
                {
                    if (Set(ref count, value)) {
                        RaisePropertyChanged(() => Description);
                    }
                }
            }

            public bool Active
            {
                get { return active; }
                set { Set(ref active, value); }
            }

            public bool IsAll
            {
                get { return isAll; }
                set { Set(ref isAll, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", name, Count); }
            }
        }
    }
}
