using Autofac;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.ViewModel;
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
            set { DataContext = value; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = ViewModelLocator.Container.Resolve<SearchViewModel>();
            ViewModel.IsInDefaultLayout = DetermineVisualState(Window.Current.Bounds.Width) == "DefaultLayout";
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        protected override void LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO set filter year etc.?
            //ViewModel.LoadFilters();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.SizeChanged -= WindowSizeChanged;
            ViewModel.Dispose();
            ViewModel = null;
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            ViewModel.IsInDefaultLayout = DetermineVisualState(Window.Current.Bounds.Width) == "DefaultLayout";
        }

        protected override string DetermineVisualState(double width)
        {
            return (width <= 800) ? "TightLayout" : "DefaultLayout";
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SetActiveFilter(e.AddedItems.FirstOrDefault() as SearchViewModel.Filter);
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

        // TODO should work anyway
        private void queryText_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            //await ViewModel.RefreshResults();
        }
    }
}
