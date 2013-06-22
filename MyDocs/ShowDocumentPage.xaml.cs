using MyDocs.Common.Contract.Page;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStoreFrontend.Common;
using MyDocs.WindowsStoreFrontend.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStoreFrontend
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
			get { return this.DataContext as DocumentViewModel; }
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			dtm = DataTransferManager.GetForCurrentView();
			dtm.DataRequested += dtm_DataRequested;
		}

		private void dtm_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
		{
			string fileTitle = ViewModel.SelectedDocument.TagsString;

			DataPackage data = args.Request.Data;
			data.Properties.Title = fileTitle;

			DataRequestDeferral waiter = args.Request.GetDeferral();

			try {
				var files = ViewModel.SelectedDocument.Photos.Select(p => p.File);
				data.SetStorageItems(files.Select(f => ((WindowsStoreFile)f).File));
			}
			finally {
				waiter.Complete();
			}
		}

		protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
		{
			if (navigationParameter != null) {
				ViewModel.SelectedDocumentId = (Guid)navigationParameter;
			}
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			dtm.DataRequested -= dtm_DataRequested;
		}
	}
}
