using MyDocs.Model;
using MyDocs.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyDocs
{
	public sealed partial class SearchResultsPage : MyDocs.Common.LayoutAwarePage
	{

		public SearchResultsPage()
		{
			this.InitializeComponent();
		}

		public SearchViewModel ViewModel
		{
			get { return this.DataContext as SearchViewModel; }
		}

		protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
		{
			var queryText = navigationParameter as string;

			// TODO: Application-specific searching logic.  The search process is responsible for
			//       creating a list of user-selectable result categories:
			//
			//       filterList.Add(new Filter("<filter name>", <result count>));
			//
			//       Only the first filter, typically "All", should pass true as a third argument in
			//       order to start in an active state.  Results for the active filter are provided
			//       in Filter_SelectionChanged below.

			ViewModel.QueryText = queryText;
			IList<SearchViewModel.Filter> filters = new ObservableCollection<SearchViewModel.Filter> {
				new SearchViewModel.Filter("All", active: true)
			};
			foreach (string categoryName in ViewModel.CategoryNames) {
				filters.Add(new SearchViewModel.Filter(categoryName));
			}
			ViewModel.Filters = filters;
		}

		/// <summary>
		/// Invoked when a filter is selected using the ComboBox in snapped view state.
		/// </summary>
		/// <param name="sender">The ComboBox instance.</param>
		/// <param name="e">Event data describing how the selected filter was changed.</param>
		private async void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Determine what filter was selected
			var selectedFilter = e.AddedItems.FirstOrDefault() as SearchViewModel.Filter;
			if (selectedFilter != null) {
				// Mirror the results into the corresponding Filter object to allow the
				// RadioButton representation used when not snapped to reflect the change
				selectedFilter.Active = true;

				await ViewModel.RefreshResults();

				// Ensure results are found
				if (ViewModel.HasResults) {
					VisualStateManager.GoToState(this, "ResultsFound", true);
					return;
				}
			}

			// Display informational text when there are no search results.
			VisualStateManager.GoToState(this, "NoResultsFound", true);
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
				filtersViewSource.View.MoveCurrentTo(filter);
			}
		}

		private void OnDocumentClick(object sender, ItemClickEventArgs e)
		{
			// delegate
			ViewModel.ShowDocumentCommand.Execute((Document)e.ClickedItem);
		}
	}
}
