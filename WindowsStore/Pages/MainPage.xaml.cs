using GalaSoft.MvvmLight.Messaging;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Messages;
using MyDocs.Common.Model;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStore.Pages
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
			this.semanticZoomTight.ViewChangeCompleted += semanticZoom_ViewChangeCompleted;

			Window.Current.SizeChanged += WindowSizeChanged;

			Messenger.Default.Register<CloseFlyoutsMessage>(this, m => CloseFlyouts());

			RefreshLayout();
		}

		private void CloseFlyouts()
		{
			foreach (var flyout in GetFlyouts(this).Concat(GetFlyouts(BottomAppBar))) {
				flyout.Hide();
			}
		}

		private IEnumerable<FlyoutBase> GetFlyouts(DependencyObject parent)
		{
			return from child in VisualTreeHelperEx.GetChildren(parent)
				   where child is Button
				   let button = (Button)child
				   where button.Flyout != null
				   select button.Flyout;
		}

		protected override void LoadState(object sender, LoadStateEventArgs args)
		{
			ViewModel.LoadAsync().ContinueWith(t =>
			{
				if (t.IsFaulted) {
					// TODO show error
				}
				else {
					if (args.NavigationParameter != null) {
						ViewModel.SelectedDocumentId = (Guid)args.NavigationParameter;
					}
					if (groupedDocumentsViewSource.View != null) {
						var collectionGroups = groupedDocumentsViewSource.View.CollectionGroups;
						((ListViewBase)this.semanticZoom.ZoomedOutView).ItemsSource = collectionGroups;
						((ListViewBase)this.semanticZoomTight.ZoomedOutView).ItemsSource = collectionGroups;
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
			bool zoomedIn = (ApplicationView.GetForCurrentView().IsFullScreen ?
				this.semanticZoom.IsZoomedInViewActive :
				this.semanticZoomTight.IsZoomedInViewActive);
			//this.editDocButton.IsEnabled = zoomedIn;
			//this.deleteDocButton.IsEnabled = zoomedIn;

			ViewModel.InZoomedInView = zoomedIn;
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			this.semanticZoom.ViewChangeCompleted -= semanticZoom_ViewChangeCompleted;
			this.semanticZoomTight.ViewChangeCompleted -= semanticZoom_ViewChangeCompleted;

			Window.Current.SizeChanged -= WindowSizeChanged;

			Messenger.Default.Unregister<CloseFlyoutsMessage>(this);
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
