﻿using Autofac;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model.View;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStore.Pages
{
    public sealed partial class ShowDocumentPage : LayoutAwarePage, IShowDocumentPage
    {
        public ShowDocumentPage()
        {
            InitializeComponent();
        }

        public DocumentViewModel ViewModel
        {
            get { return (DocumentViewModel)DataContext; }
            private set { DataContext = value; }
        }

        protected override IEnumerable<IDisposable> Activate()
        {
            yield return ViewModel = ViewModelLocator.Container.Resolve<DocumentViewModel>();

            var dtm = DataTransferManager.GetForCurrentView();
            yield return Observable.FromEventPattern<TypedEventHandler<DataTransferManager, DataRequestedEventArgs>, DataRequestedEventArgs>(
                h => dtm.DataRequested += h,
                h => dtm.DataRequested -= h)
                .Subscribe(e => TransferData(e.EventArgs));

            yield return Observable.FromEventPattern<WindowSizeChangedEventHandler, WindowSizeChangedEventArgs>(
                h => Window.Current.SizeChanged += h,
                h => Window.Current.SizeChanged -= h)
                .Subscribe(e => OnWindowSizeChanged(e.EventArgs.Size));
        }

        private void TransferData(DataRequestedEventArgs args)
        {
            var document = ViewModel.SelectedDocument;
            string fileTitle = string.Join("_", document.Tags);

            var data = args.Request.Data;
            data.Properties.Title = fileTitle;

            var waiter = args.Request.GetDeferral();

            try {
                var files = document.SubDocuments.Select(p => p.File);
                data.SetStorageItems(files);
            }
            finally {
                waiter.Complete();
            }
        }

        protected override void LoadState(object sender, LoadStateEventArgs args)
        {
            // unify with `MainPage.LoadState`
            var selectedDocumentId = (Guid?)args.NavigationParameter;
            if (selectedDocumentId.HasValue)
            {
                ViewModel.WhenAnyValue(x => x.Categories)
                    .TakeWhile(_ => ViewModel.SelectedDocument == null)
                    .Subscribe(_ => ViewModel.TrySelectDocument(selectedDocumentId.Value));
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.GoBack();
        }

        // http://stackoverflow.com/questions/14667556/a-simple-photo-album-with-pinch-and-zoom-using-flipview
        private void OnWindowSizeChanged(Size size)
        {
            // Reset scroll view size
            var flipViewItems = Enumerable.Range(0, imageFlipView.Items.Count)
                .Select(i => imageFlipView.ContainerFromIndex(i))
                .Where(container => container != null);
            foreach (var flipViewItem in flipViewItems) {
                var scrollViewItem = VisualTreeHelperEx.GetChildren(flipViewItem).OfType<ScrollViewer>().First();
                scrollViewItem.ChangeView(0, 0, 1, true);

                // TODO size should automatically adapt because of binding
                var filePreview = VisualTreeHelperEx.GetChildren(flipViewItem).OfType<MyDocs.WindowsStore.Controls.FilePreview>().Single();
                filePreview.Width = size.Width;
                filePreview.Height = size.Height;
            }
        }

        // http://stackoverflow.com/questions/14667556/a-simple-photo-album-with-pinch-and-zoom-using-flipview
        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var flipView = (FlipView)sender;
            var flipViewItem = flipView.ContainerFromIndex(flipView.SelectedIndex);
            if (flipViewItem != null) {
                var scrollViewItem = VisualTreeHelperEx.GetChildren(flipViewItem).OfType<ScrollViewer>().First();
                scrollViewItem.ChangeView(0, 0, 1, true);
            }
        }
    }
}
