﻿using GalaSoft.MvvmLight;
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
            set
            {
                if (queryText != value) {
                    queryText = value;
                    RaisePropertyChanged(() => QueryText);

                    var t = RefreshResults();
                }
            }
        }

        public IList<Filter> Filters
        {
            get { return filters; }
            set
            {
                if (filters != value) {
                    filters = value;
                    RaisePropertyChanged(() => Filters);
                }
            }
        }

        public IEnumerable<Document> Results
        {
            get { return results; }
            set
            {
                if (results != value) {
                    results = value;
                    RaisePropertyChanged(() => Results);
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

        public SearchViewModel(IDocumentService documentService, INavigationService navigationService)
        {
            this.documentService = documentService;
            this.navigationService = navigationService;

            queryText = "";
            Filters = new ObservableCollection<Filter> {
                new SearchViewModel.Filter("All", active: true)
            };
            foreach (string categoryName in CategoryNames) {
                Filters.Add(new SearchViewModel.Filter(categoryName));
            }

            CreateCommands();
            CreateDesignTimeData();
        }

        #region Commands

        public RelayCommand<Document> ShowDocumentCommand { get; set; }

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
                Filters = new List<SearchViewModel.Filter> {
                    new SearchViewModel.Filter("All", active: true)
                };
                var t = RefreshResults();
            }
        }

        public async Task RefreshResults()
        {
            var searchWords = QueryText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            await documentService.LoadCategoriesAsync();

            var docs = (from category in documentService.Categories
                        from doc in category.Documents
                        from word in searchWords
                        where doc.Tags.Any(t => t.ToLower().Contains(word))
                        select doc).ToList();
            foreach (var filter in Filters) {
                IEnumerable<Document> results;
                if (filter.Name == "All") {
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

            public Filter(string name, int count = 0, bool active = false)
            {
                Name = name;
                Count = count;
                Active = active;
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
                    if (name != value) {
                        name = value;
                        RaisePropertyChanged(() => Name);
                        RaisePropertyChanged(() => Description);
                    }
                }
            }

            public int Count
            {
                get { return count; }
                set
                {
                    if (count != value) {
                        count = value;
                        RaisePropertyChanged(() => Count);
                        RaisePropertyChanged(() => Description);
                    }
                }
            }

            public bool Active
            {
                get { return active; }
                set
                {
                    if (active != value) {
                        active = value;
                        RaisePropertyChanged(() => Active);
                    }
                }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", name, Count); }
            }
        }
    }
}
