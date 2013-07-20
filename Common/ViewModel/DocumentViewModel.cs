using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Collection;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MyDocs.Common.ViewModel
{
	public class DocumentViewModel : ViewModelBase
	{
		private readonly IDocumentService documentService;
		private readonly INavigationService navigationService;
		private readonly IUserInterfaceService uiService;

		#region Properties

		//private ObservableCollection<Category> categories;
		private Document selectedDocument;
		private Category selectedCategory;
		private bool isLoaded = false;
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

		public bool IsLoaded
		{
			get { return isLoaded; }
			set
			{
				if (isLoaded != value) {
					isLoaded = value;
					RaisePropertyChanged(() => IsLoaded);
					RaisePropertyChanged(() => Categories);
					RaisePropertyChanged(() => CategoriesEmpty);
					RaisePropertyChanged(() => CategoriesNotEmpty);
				}
			}
		}

		public SortedObservableCollection<Category> Categories
		{
			get { return documentService.Categories; }
		}

		public bool CategoriesEmpty
		{
			get { return IsLoaded && Categories.Count == 0; }
		}
		public bool CategoriesNotEmpty { get { return !CategoriesEmpty; } }

		public Document SelectedDocument
		{
			get { return selectedDocument; }
			set
			{
				if (selectedDocument != value) {
					selectedDocument = value;
					RaisePropertyChanged(() => SelectedDocument);
					RaisePropertyChanged(() => HasSelectedDocument);

					DeleteDocumentCommand.RaiseCanExecuteChanged();
					EditDocumentCommand.RaiseCanExecuteChanged();
				}
			}
		}

		public bool HasSelectedDocument { get { return SelectedDocument != null; } }

		public Guid SelectedDocumentId
		{
			set
			{
				SelectedDocument = null;
				documentService.GetDocumentById(value).ContinueWith(t =>
				{
					if (t.IsFaulted) {
						// TODO show error
						SelectedDocument = new Document();
					}
					else {
						SelectedDocument = t.Result;
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		public Category SelectedCategory
		{
			get { return selectedCategory; }
			set
			{
				if (selectedCategory != value) {
					selectedCategory = value;
					RaisePropertyChanged(() => SelectedCategory);
					RaisePropertyChanged(() => HasSelectedCategory);

					RenameCategoryCommand.RaiseCanExecuteChanged();
					DeleteCategoryCommand.RaiseCanExecuteChanged();
				}
			}
		}

		public bool HasSelectedCategory { get { return SelectedCategory != null; } }

		#endregion

		public DocumentViewModel(IDocumentService documentService,
			IUserInterfaceService uiService,
			INavigationService navigationService)
		{
			this.documentService = documentService;
			this.navigationService = navigationService;
			this.uiService = uiService;

			CreateCommands();
			CreateDesignTimeData();

			InZoomedInView = true;
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
			IsLoaded = true;
		}

		#region Commands

		public RelayCommand AddDocumentCommand { get; set; }
		public RelayCommand EditDocumentCommand { get; set; }
		public RelayCommand DeleteDocumentCommand { get; set; }
		public RelayCommand<Document> ShowDocumentCommand { get; set; }
		public RelayCommand RenameCategoryCommand { get; set; }
		public RelayCommand DeleteCategoryCommand { get; set; }

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

			RenameCategoryCommand = new RelayCommand(RenameCategoryCommandHandler/*, () => SelectedCategory != null*/);
			DeleteCategoryCommand = new RelayCommand(DeleteCategoryCommandHandler/*, () => SelectedCategory != null*/);
		}

		private void AddDocumentCommandHandler()
		{
			navigationService.Navigate(typeof(IEditDocumentPage));
		}

		private void EditDocumentCommandHandler()
		{
			navigationService.Navigate(typeof(IEditDocumentPage), SelectedDocument.Id);
		}

		private void DeleteDocumentHandler()
		{
			documentService.DeleteDocumentAsync(SelectedDocument).ContinueWith(t =>
			{
				if (t.IsFaulted) {
					var tmp = uiService.ShowErrorAsync("deleteDocError");
				}
				RaisePropertyChanged(() => CategoriesEmpty);
				RaisePropertyChanged(() => CategoriesNotEmpty);
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void RenameCategoryCommandHandler()
		{
			throw new NotImplementedException();
		}

		private void DeleteCategoryCommandHandler()
		{
			throw new NotImplementedException();
		}

		private void ShowDocumentCommandHandler(Document doc)
		{
			navigationService.Navigate(typeof(IShowDocumentPage), doc.Id);
		}

		#endregion
	}
}