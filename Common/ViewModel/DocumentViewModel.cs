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
        private readonly ITranslatorService translatorService;

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
                        uiService.ShowErrorAsync("documentNotFound");
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
            IFileSavePickerService fileSavePickerService,
            ITranslatorService translatorService)
        {
            this.documentService = documentService;
            this.navigationService = navigationService;
            this.uiService = uiService;
            this.licenseService = licenseService;
            this.fileSavePickerService = fileSavePickerService;
            this.translatorService = translatorService;

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
            bool error;
            try {
                await documentService.LoadCategoriesAsync();
                error = false;
            }
            catch (Exception) {
                error = true;
            }
            IsLoaded = true;
            if (error) {
                await uiService.ShowErrorAsync("loadDocumentsError");
            }
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
            AddDocumentCommand = new RelayCommand(AddDocument);
            EditDocumentCommand = new RelayCommand(EditDocument,
                () => SelectedDocument != null && !(SelectedDocument is AdDocument));
            DeleteDocumentCommand = new RelayCommand(DeleteDocumentAsync,
                () => SelectedDocument != null && !(SelectedDocument is AdDocument));
            ShowDocumentCommand = new RelayCommand<Document>(
                doc => navigationService.Navigate<IShowDocumentPage>(doc.Id),
                doc => doc != null && !(doc is AdDocument));

            RenameCategoryCommand = new RelayCommand<Category>(RenameCategoryAsync);
            DeleteCategoryCommand = new RelayCommand<Category>(DeleteCategoryAsync);

            NavigateToSearchPageCommand = new RelayCommand(() => navigationService.Navigate<ISearchPage>());

            ExportDocumentsCommand = new RelayCommand(ExportDocumentsAsync);
        }

        private void AddDocument()
        {
            navigationService.Navigate<IEditDocumentPage>();
        }

        private void EditDocument()
        {
            navigationService.Navigate<IEditDocumentPage>(SelectedDocument.Id);
        }

        private async void DeleteDocumentAsync()
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            await documentService.DeleteDocumentAsync(SelectedDocument);

            RaisePropertyChanged(() => CategoriesEmpty);
            RaisePropertyChanged(() => CategoriesNotEmpty);
        }

        private async void RenameCategoryAsync(Category cat)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            await documentService.RenameCategoryAsync(cat, NewCategoryName);

            RaisePropertyChanged(() => CategoriesEmpty);
            RaisePropertyChanged(() => CategoriesNotEmpty);
        }

        private async void DeleteCategoryAsync(Category cat)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            var tasks = cat.Documents.Where(d => !(d is AdDocument)).ToList().Select(d => documentService.DeleteDocumentAsync(d));
            await Task.WhenAll(tasks);

            RaisePropertyChanged(() => CategoriesEmpty);
            RaisePropertyChanged(() => CategoriesNotEmpty);
        }

        private async void ExportDocumentsAsync()
        {
            var status = await licenseService.TryGetLicenseAsync("ExportImportDocuments");
            if (status == LicenseStatus.Unlocked) {
                var fileTypes = new Dictionary<string, IList<string>> {
                    { translatorService.Translate("archive"), new List<string> { ".zip" } }
                };
                var savedFiles = new HashSet<string>();
                var zipFile = await fileSavePickerService.PickSaveFileAsync("MyDocs.zip", fileTypes);
                if (zipFile != null) {
                    using (var zipFileStream = await zipFile.OpenWriteAsync()) {
                        using (ZipArchive archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create)) {
                            foreach (var document in documentService.Categories.SelectMany(c => c.Documents)) {
                                foreach (var photo in document.Photos) {
                                    var tags = document.Tags.Select(t => RemoveInvalidFileNameChars(t));
                                    var fileName = photo.Title != null ? String.Format("{0}{1}", photo.Title, Path.GetExtension(photo.File.Name)) : photo.File.Name;
                                    var path = Path.Combine(String.Format("{0} ({1})", String.Join("-", tags), document.Id), fileName);
                                    if (!savedFiles.Contains(path)) {
                                        var entry = archive.CreateEntry(path);
                                        using (var reader = await photo.File.OpenReadAsync())
                                        using (var writer = entry.Open()) {
                                            await reader.CopyToAsync(writer);
                                        }
                                        savedFiles.Add(path);
                                    }
                                }
                            }
                        }
                    }
                    await uiService.ShowNotificationAsync("exportFinished");
                }
            }
            else if (status == LicenseStatus.Locked) {
                await uiService.ShowErrorAsync("exportLocked");
            }
            else if (status == LicenseStatus.Error) {
                await uiService.ShowErrorAsync("exportUnlockError");
            }
        }

        private string RemoveInvalidFileNameChars(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        #endregion
    }
}