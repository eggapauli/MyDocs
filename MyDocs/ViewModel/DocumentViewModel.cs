using MyDocs.Common;
using MyDocs.Contract;
using MyDocs.Contract.Service;
using MyDocs.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.ViewModel
{
	public class DocumentViewModel : ViewModelBase
	{
		private readonly IDocumentService documentService;
		private readonly INavigationService navigationService;

		private ResourceLoader rl = new ResourceLoader();

		#region Properties

		//private ObservableCollection<Category> categories;
		private Document selectedDocument;
		private bool inZoomedInView;

		public bool InZoomedInView
		{
			get { return inZoomedInView; }
			set
			{
				if (inZoomedInView != value) {
					inZoomedInView = value;
					RaisePropertyChanged(() => InZoomedInView);
					RaisePropertyChanged(() => InZoomedOutView);
				}
			}
		}

		public bool InZoomedOutView { get { return !InZoomedInView; } }

		public IEnumerable<Category> Categories
		{
			get { return documentService.Categories; }
			//set
			//{
			//	if (categories != value) {
			//		categories = value;
			//		RaisePropertyChanged(() => Categories);
			//		RaisePropertyChanged(() => CategoriesEmpty);
			//		RaisePropertyChanged(() => CategoriesNotEmpty);
			//	}
			//}
		}

		public bool CategoriesEmpty { get { return Categories == null || !Categories.Any(); } }
		public bool CategoriesNotEmpty { get { return !CategoriesEmpty; } }

		public Document SelectedDocument
		{
			get { return selectedDocument; }
			set
			{
				if (selectedDocument != value) {
					selectedDocument = value;
					RaisePropertyChanged(() => SelectedDocument);

					DeleteDocumentCommand.RaiseCanExecuteChanged();
					EditDocumentCommand.RaiseCanExecuteChanged();
				}
			}
		}

		#endregion

		public DocumentViewModel(IDocumentService documentService, INavigationService navigationService)
		{
			this.documentService = documentService;
			this.navigationService = navigationService;

			CreateCommands();
			CreateDesignTimeData();
		}

		[Conditional("DEBUG")]
		private void CreateDesignTimeData()
		{
			if (IsInDesignMode) {
				LoadAsync().ContinueWith(t =>
				{
					SelectedDocument = Categories.First().Documents.First(d => d.Tags.Count > 2);
				}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		public async Task LoadAsync()
		{
			await documentService.LoadCategoriesAsync();
			//Categories = new ObservableCollection<Category>(documentService.Categories);
		}

		#region Commands

		public RelayCommand AddDocumentCommand { get; set; }
		public RelayCommand EditDocumentCommand { get; set; }
		public RelayCommand DeleteDocumentCommand { get; set; }
		public RelayCommand<Document> ShowDocumentCommand { get; set; }

		private void CreateCommands()
		{
			AddDocumentCommand = new RelayCommand(AddDocumentCommandHandler);
			EditDocumentCommand = new RelayCommand(EditDocumentCommandHandler, () =>
				SelectedDocument != null
				&& !(SelectedDocument is AdDocument));
			DeleteDocumentCommand = new RelayCommand(DeleteDocumentHandler, () =>
				SelectedDocument != null
				&& !(SelectedDocument is AdDocument));
			ShowDocumentCommand = new RelayCommand<Document>(ShowDocumentCommandHandler, doc =>
				doc != null
				&& !(doc is AdDocument));
		}

		private void AddDocumentCommandHandler()
		{
			SelectedDocument = new Document();
			navigationService.Navigate(typeof(EditDocumentPage), SelectedDocument);
		}

		private void EditDocumentCommandHandler()
		{
			navigationService.Navigate(typeof(EditDocumentPage), SelectedDocument);
		}

		private void DeleteDocumentHandler()
		{
			documentService.DeleteDocumentAsync(SelectedDocument).ContinueWith(t =>
			{
				if (t.IsFaulted) {
					var tmp = ShowErrorAsync("deleteDocError");
				}
				RaisePropertyChanged(() => CategoriesEmpty);
				RaisePropertyChanged(() => CategoriesNotEmpty);
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void ShowDocumentCommandHandler(Document doc)
		{
			navigationService.Navigate(typeof(ShowDocumentPage), doc);
		}

		#endregion

		#region Helper

		// TODO remove UI logic from ViewModel
		private async Task<IUICommand> ShowErrorAsync(string msgKey)
		{
			string msg = rl.GetString(msgKey);
			if (String.IsNullOrEmpty(msg)) {
				msg = "An error occured.";
			}
			return await new MessageDialog(msg).ShowAsync().AsTask();
		}

		#endregion
	}
}