using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Collection;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Messages;
using MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace MyDocs.Common.ViewModel
{
    public class DocumentViewModel : ViewModelBase
    {
        private readonly IDocumentService documentService;
        private readonly INavigationService navigationService;
        private readonly IUserInterfaceService uiService;
        private readonly ILicenseService licenseService;
        private readonly IFileSavePickerService fileSavePickerService;

        #region Properties

        //private ObservableCollection<Category> categories;
        private Document selectedDocument;
        private Category selectedCategory;
        private string newCategoryName;
        private bool inCategoryEditMode;
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

        public string NewCategoryName
        {
            get { return newCategoryName; }
            set
            {
                if (newCategoryName != value) {
                    newCategoryName = value;
                    RaisePropertyChanged(() => NewCategoryName);
                }
            }
        }

        public bool InEditCategoryMode
        {
            get { return inCategoryEditMode; }
            set
            {
                if (inCategoryEditMode != value) {
                    inCategoryEditMode = value;
                    RaisePropertyChanged(() => InEditCategoryMode);
                }
            }
        }

        #endregion

        public DocumentViewModel(IDocumentService documentService,
            IUserInterfaceService uiService,
            INavigationService navigationService,
            ILicenseService licenseService,
            IFileSavePickerService fileSavePickerService)
        {
            this.documentService = documentService;
            this.navigationService = navigationService;
            this.uiService = uiService;
            this.licenseService = licenseService;
            this.fileSavePickerService = fileSavePickerService;

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
        public RelayCommand<Category> RenameCategoryCommand { get; set; }
        public RelayCommand<Category> DeleteCategoryCommand { get; set; }
        public RelayCommand NavigateToSearchPageCommand { get; set; }
        public RelayCommand ExportDocumentsCommand { get; set; }

        private void CreateCommands()
        {
            AddDocumentCommand = new RelayCommand(AddDocumentCommandHandler);
            EditDocumentCommand = new RelayCommand(EditDocumentCommandHandler,
                () => SelectedDocument != null && !(SelectedDocument is AdDocument));
            DeleteDocumentCommand = new RelayCommand(DeleteDocumentHandler,
                () => SelectedDocument != null && !(SelectedDocument is AdDocument));
            ShowDocumentCommand = new RelayCommand<Document>(
                doc => navigationService.Navigate(typeof(IShowDocumentPage), doc.Id),
                doc => doc != null && !(doc is AdDocument));

            RenameCategoryCommand = new RelayCommand<Category>(RenameCategoryCommandHandler);
            DeleteCategoryCommand = new RelayCommand<Category>(DeleteCategoryCommandHandler);

            NavigateToSearchPageCommand = new RelayCommand(() => navigationService.Navigate(typeof(ISearchPage)));

            ExportDocumentsCommand = new RelayCommand(ExportDocuments);
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
            MessengerInstance.Send(new CloseFlyoutsMessage());

            documentService.DeleteDocumentAsync(SelectedDocument).ContinueWith(t =>
            {
                if (t.IsFaulted) {
                    var tmp = uiService.ShowErrorAsync("deleteDocError");
                }
                RaisePropertyChanged(() => CategoriesEmpty);
                RaisePropertyChanged(() => CategoriesNotEmpty);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void RenameCategoryCommandHandler(Category cat)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            if (cat.Name == NewCategoryName) {
                return;
            }

            foreach (var doc in cat.Documents) {
                doc.Category = NewCategoryName;
            }

            var existingCat = Categories.FirstOrDefault(c => c.Name == NewCategoryName);
            if (existingCat != null) {
                foreach (var doc in cat.Documents) {
                    existingCat.Documents.Add(doc);
                }
                Categories.Remove(cat);
            }
            else {
                cat.Name = NewCategoryName;
                // Re-sort
                Categories.Remove(cat);
                Categories.Add(cat);
            }

            var tasks = cat.Documents.Select(d => documentService.SaveDocumentAsync(d));
            Task.WhenAll(tasks).ContinueWith(t =>
            {
                if (t.IsFaulted) {
                    var tmp = uiService.ShowErrorAsync("renameCatError");
                }
                RaisePropertyChanged(() => CategoriesEmpty);
                RaisePropertyChanged(() => CategoriesNotEmpty);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DeleteCategoryCommandHandler(Category cat)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            var tasks = cat.Documents.Where(d => !(d is AdDocument)).ToList().Select(d => documentService.DeleteDocumentAsync(d));
            Task.WhenAll(tasks).ContinueWith(t =>
            {
                if (t.IsFaulted) {
                    var tmp = uiService.ShowErrorAsync("deleteCatError");
                }
                RaisePropertyChanged(() => CategoriesEmpty);
                RaisePropertyChanged(() => CategoriesNotEmpty);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void ExportDocuments()
        {
            if (!await licenseService.TryGetLicenseAsync("ExportImportDocuments")) {
                // TODO show error
            }
            else {
                var fileTypes = new Dictionary<string, IList<string>> {
                    { "Archive", new List<string> { ".zip" } } // TODO translate
                };
                var zipFile = await fileSavePickerService.PickSaveFileAsync("MyDocs.zip", fileTypes);
                using (var zipFileStream = await zipFile.OpenWriteAsync()) {
                    using (ZipArchive archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create)) {
                        foreach (var document in documentService.Categories.SelectMany(c => c.Documents)) {
                            foreach (var file in document.Photos.Select(p => p.File).Distinct()) {
                                var tags = document.Tags.Select(t => RemoveInvalidFileNameChars(t));
                                var entry = archive.CreateEntry(Path.Combine(String.Join("-", tags), file.Name));
                                using (var reader = await file.OpenReadAsync())
                                using (var writer = entry.Open()) {
                                    await reader.CopyToAsync(writer);
                                }
                            }
                        }
                    }
                }
            }
        }

        private string RemoveInvalidFileNameChars(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        #endregion
    }
}