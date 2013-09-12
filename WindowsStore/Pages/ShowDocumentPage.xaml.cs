using MyDocs.Common.Contract.Page;
using MyDocs.Common.Model;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Common;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
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
        }

        private void backButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            NavigationHelper.GoBack();
        }
    }
}
