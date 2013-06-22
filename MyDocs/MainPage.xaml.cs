using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStoreFrontend.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStoreFrontend
{
	public sealed partial class MainPage : LayoutAwarePage, IMainPage
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		public DocumentViewModel ViewModel
		{
			get { return this.DataContext as DocumentViewModel; }
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			this.semanticZoom.ViewChangeCompleted += semanticZoom_ViewChangeCompleted;
			this.semanticZoomSnapped.ViewChangeCompleted += semanticZoom_ViewChangeCompleted;

			Window.Current.SizeChanged += WindowSizeChanged;

			if (ViewModel.CategoriesEmpty) {
				appBar.IsOpen = true;
			}

			RefreshLayout();
		}

		protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
		{
			ViewModel.LoadAsync().ContinueWith(t =>
			{
				if (t.IsFaulted) {
					// TODO show error
				}
				else {
					if (navigationParameter != null) {
						ViewModel.SelectedDocumentId = (Guid)navigationParameter;
					}
					if (groupedDocumentsViewSource.View != null) {
						var collectionGroups = groupedDocumentsViewSource.View.CollectionGroups;
						((ListViewBase)this.semanticZoom.ZoomedOutView).ItemsSource = collectionGroups;
						((ListViewBase)this.semanticZoomSnapped.ZoomedOutView).ItemsSource = collectionGroups;
					}
				}
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
		{
			RefreshLayout();
		}

		private void RefreshLayout()
		{
			bool zoomedIn = (ApplicationView.Value == ApplicationViewState.Snapped ?
				this.semanticZoomSnapped.IsZoomedInViewActive :
				this.semanticZoom.IsZoomedInViewActive);
			this.editDocButton.IsEnabled = zoomedIn;
			this.deleteDocButton.IsEnabled = zoomedIn;

			ViewModel.InZoomedInView = zoomedIn;
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			this.semanticZoom.ViewChangeCompleted -= semanticZoom_ViewChangeCompleted;
			this.semanticZoomSnapped.ViewChangeCompleted -= semanticZoom_ViewChangeCompleted;

			Window.Current.SizeChanged -= WindowSizeChanged;
		}

		// TODO solve this in XAML using <VisualStateGroup x:Name="SemanticZoomStates" />
		private void semanticZoom_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
		{
			// synchronize semantic zooms - strange behavior
			//if (sender as SemanticZoom == this.semanticZoom) {
			//	this.semanticZoomSnapped.IsZoomedInViewActive = this.semanticZoom.IsZoomedInViewActive;
			//}
			//else {
			//	this.semanticZoom.IsZoomedInViewActive = this.semanticZoomSnapped.IsZoomedInViewActive;
			//}
			RefreshLayout();
		}

		private void OnDocumentClick(object sender, ItemClickEventArgs e)
		{
			// delegate
			ViewModel.ShowDocumentCommand.Execute((Document)e.ClickedItem);
		}
	}
}
