using Autofac;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model.View;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace MyDocs.WindowsStore.Pages
{
    public sealed partial class MainPage : LayoutAwarePage, IMainPage
    {
        private DocumentViewModel ViewModel
        {
            get { return (DocumentViewModel)DataContext; }
            set { DataContext = value; }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        protected override IEnumerable<IDisposable> Activate()
        {
            yield return ViewModel = ViewModelLocator.Container.Resolve<DocumentViewModel>();

            // TODO solve this in XAML using <VisualStateGroup x:Name="SemanticZoomStates" />
            yield return Observable.FromEventPattern<SemanticZoomViewChangedEventHandler, SemanticZoomViewChangedEventArgs>(
                h => semanticZoom.ViewChangeCompleted += h,
                h => semanticZoom.ViewChangeCompleted -= h
            ).Subscribe(_ => RefreshLayout());

            yield return Observable.FromEventPattern<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                h => Window.Current.SizeChanged += h,
                h => Window.Current.SizeChanged -= h
            )
            .Select(_ => Unit.Default)
            .StartWith(Unit.Default)
            .Subscribe(_ => RefreshLayout());

            yield return ViewModel.CloseFlyoutsMessages.Subscribe(_ => CloseFlyouts());
        }

        private void CloseFlyouts()
        {
            foreach (var flyout in GetFlyouts(this).Concat(GetFlyouts(BottomAppBar)))
            {
                flyout.Hide();
            }
        }

        private static IEnumerable<FlyoutBase> GetFlyouts(DependencyObject parent)
        {
            return from child in VisualTreeHelperEx.GetChildren(parent)
                   where child is Button
                   let button = (Button)child
                   where button.Flyout != null
                   select button.Flyout;
        }

        protected override async void LoadState(object sender, LoadStateEventArgs args)
        {
            ViewModel.IsBusy = true;
            using (Disposable.Create(() => ViewModel.IsBusy = false)) {
                await MigrationHelper.Migrate();
            }

            // unify with `ShowDocumentPage.LoadState`
            var selectedDocumentId = (Guid?)args.NavigationParameter;
            if (selectedDocumentId.HasValue) {
                ViewModel.WhenAnyValue(x => x.Categories)
                    .TakeWhile(_ => ViewModel.SelectedDocument == null)
                    .Subscribe(_ => ViewModel.TrySelectDocument(selectedDocumentId.Value));
            }

            if (groupedDocumentsViewSource.View != null) {
                var collectionGroups = groupedDocumentsViewSource.View.CollectionGroups;
                ((ListViewBase)this.semanticZoom.ZoomedOutView).ItemsSource = collectionGroups;
                ((ListViewBase)this.semanticZoomTight.ZoomedOutView).ItemsSource = collectionGroups;
            }
        }

        private void RefreshLayout()
        {
            bool zoomedIn = (ApplicationView.GetForCurrentView().IsFullScreen ?
                this.semanticZoom.IsZoomedInViewActive :
                this.semanticZoomTight.IsZoomedInViewActive);

            ViewModel.InZoomedInView = zoomedIn;
        }

        private void OnDocumentClick(object sender, ItemClickEventArgs e)
        {
            var document = e.ClickedItem as Document;
            if (document != null) {
                ViewModel.ShowDocumentCommand.Execute(document);
            }
        }
    }
}
