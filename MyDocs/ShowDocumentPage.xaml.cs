using MyDocs.Common;
using MyDocs.Model;
using MyDocs.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyDocs
{
	public sealed partial class ShowDocumentPage : LayoutAwarePage
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
				data.SetStorageItems(files);
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
