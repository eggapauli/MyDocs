using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStore.Pages
{
    public sealed partial class SearchResultsPage : LayoutAwarePage, ISearchPage
    {
        public SearchResultsPage()
        {
            this.InitializeComponent();
        }

        public SearchViewModel ViewModel
        {
            get { return (SearchViewModel)DataContext; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.IsInDefaultLayout = DetermineVisualState(Window.Current.Bounds.Width) == "DefaultLayout";
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            ViewModel.IsInDefaultLayout = DetermineVisualState(Window.Current.Bounds.Width) == "DefaultLayout";
        }

        protected override void LoadState(object sender, LoadStateEventArgs args)
        {
            //var queryText = args.NavigationParameter as string;

            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            //ViewModel.QueryText = queryText;
            IList<SearchViewModel.Filter> filters = new ObservableCollection<SearchViewModel.Filter> {
				new SearchViewModel.Filter("All", active: true)
			};
            foreach (string categoryName in ViewModel.CategoryNames) {
                filters.Add(new SearchViewModel.Filter(categoryName));
            }
            ViewModel.Filters = filters;
        }

        protected override string DetermineVisualState(double width)
        {
            return (width < 1366 / 2) ? "TightLayout" : "DefaultLayout";
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        private async void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var removedFilters = e.RemovedItems.Select(item => (SearchViewModel.Filter)item);
            foreach (var filter in removedFilters) {
                filter.Active = false;
            }

            var selectedFilter = e.AddedItems.FirstOrDefault() as SearchViewModel.Filter;
            if (selectedFilter != null) {
                selectedFilter.Active = true;
            }

            await ViewModel.RefreshResults();
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null) {
                var filter = (sender as FrameworkElement).DataContext;
                // Triggers Filter_SelectionChanged
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        private void OnDocumentClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.ShowDocumentCommand.Execute(e.ClickedItem);
        }
    }
}
