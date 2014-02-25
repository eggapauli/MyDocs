using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStore.Pages
{
    public sealed partial class ShowDocumentPage : LayoutAwarePage, IShowDocumentPage
    {
        private DataTransferManager dtm;

        public ShowDocumentPage()
        {
            this.InitializeComponent();
        }

        public DocumentViewModel ViewModel
        {
            get { return (DocumentViewModel)DataContext; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            dtm = DataTransferManager.GetForCurrentView();
            dtm.DataRequested += dtm_DataRequested;
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void dtm_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var document = ((Document)ViewModel.SelectedDocument);
            string fileTitle = document.TagsString;

            DataPackage data = args.Request.Data;
            data.Properties.Title = fileTitle;

            DataRequestDeferral waiter = args.Request.GetDeferral();

            try {
                var files = document.Photos.Select(p => p.File);
                data.SetStorageItems(files.Select(f => ((WindowsStoreFile)f).File));
            }
            finally {
                waiter.Complete();
            }
        }

        protected override void LoadState(object sender, LoadStateEventArgs args)
        {
            var t = ViewModel.LoadAsync((Guid?)args.NavigationParameter);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            dtm.DataRequested -= dtm_DataRequested;
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private void backButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            NavigationHelper.GoBack();
        }

        // http://stackoverflow.com/questions/14667556/a-simple-photo-album-with-pinch-and-zoom-using-flipview
        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            // Reset scroll view size
            var flipViewItems = Enumerable.Range(0, imageFlipView.Items.Count)
                .Select(i => imageFlipView.ContainerFromIndex(i))
                .Where(container => container != null);
            foreach (var flipViewItem in flipViewItems) {
                var scrollViewItem = VisualTreeHelperEx.GetChildren(flipViewItem).OfType<ScrollViewer>().First();
                scrollViewItem.Width = e.Size.Width;
                scrollViewItem.Height = e.Size.Height; // Reset width and height to match the new size
                System.Diagnostics.Debug.WriteLine(scrollViewItem.Width);
                System.Diagnostics.Debug.WriteLine(scrollViewItem.Height);
                System.Diagnostics.Debug.WriteLine(e.Size);
                scrollViewItem.ChangeView(0, 0, 1, true);
            }
        }

        // http://stackoverflow.com/questions/14667556/a-simple-photo-album-with-pinch-and-zoom-using-flipview
        private void FlipView_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            var flipView = (FlipView)sender;
            var flipViewItem = flipView.ContainerFromIndex(flipView.SelectedIndex);
            var scrollViewItem = VisualTreeHelperEx.GetChildren(flipViewItem).OfType<ScrollViewer>().First();
            scrollViewItem.ChangeView(0, 0, 1, true);
        }
    }
}
