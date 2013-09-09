using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Collection;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Messages;
using MyDocs.Common.Model;
using Serializable = MyDocs.Common.Model.Serializable;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
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
        private readonly IFileOpenPickerService fileOpenPickerService;
        private readonly ITranslatorService translatorService;
        private readonly IPdfService pdfService;
        private readonly ISettingsService settingsService;

        #region Properties

        private Document selectedDocument;
        private Category selectedCategory;
        private string newCategoryName;
        private bool inCategoryEditMode = false;
        private bool isLoaded = false;
        private bool inZoomedInView;
        private bool isBusy = false;

        public bool InZoomedInView
        {
            get { return inZoomedInView; }
            set
            {
                if (Set(ref inZoomedInView, value)) {
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
                if (Set(ref isLoaded, value)) {
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
                if (Set(ref selectedDocument, value)) {
                    RaisePropertyChanged(() => HasSelectedDocument);

                    DeleteDocumentCommand.RaiseCanExecuteChanged();
                    EditDocumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool HasSelectedDocument { get { return SelectedDocument != null && !(SelectedDocument is AdDocument); } }

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
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                if (Set(ref selectedCategory, value)) {
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
            set { Set(ref newCategoryName, value); }
        }

        public bool InEditCategoryMode
        {
            get { return inCategoryEditMode; }
            set { Set(ref inCategoryEditMode, value); }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set { Set(ref isBusy, value); }
        }

        #endregion

        public DocumentViewModel(IDocumentService documentService,
            IUserInterfaceService uiService,
            INavigationService navigationService,
            ILicenseService licenseService,
            IFileSavePickerService fileSavePickerService,
            IFileOpenPickerService fileOpenPickerService,
            ITranslatorService translatorService,
            IPdfService pdfService,
            ISettingsService settingsService)
        {
            this.documentService = documentService;
            this.navigationService = navigationService;
            this.uiService = uiService;
            this.licenseService = licenseService;
            this.fileSavePickerService = fileSavePickerService;
            this.fileOpenPickerService = fileOpenPickerService;
            this.translatorService = translatorService;
            this.pdfService = pdfService;
            this.settingsService = settingsService;

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
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
        }

        public async Task LoadAsync()
        {
            bool error = false;
            try {
                using (new TemporaryState(() => IsLoaded = false, () => IsLoaded = true))
                using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                    await documentService.LoadCategoriesAsync();
                }
            }
            catch (Exception) {
                error = true;
            }

            if (error) {
                await uiService.ShowErrorAsync("loadDocumentsError");
            }
        }

        #region Commands

        public RelayCommand AddDocumentCommand { get; private set; }
        public RelayCommand EditDocumentCommand { get; private set; }
        public RelayCommand DeleteDocumentCommand { get; private set; }
        public RelayCommand<Document> ShowDocumentCommand { get; private set; }
        public RelayCommand<Category> RenameCategoryCommand { get; private set; }
        public RelayCommand<Category> DeleteCategoryCommand { get; private set; }
        public RelayCommand NavigateToSearchPageCommand { get; private set; }
        public RelayCommand ExportDocumentsCommand { get; private set; }
        public RelayCommand ImportDocumentsCommand { get; private set; }

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
            ImportDocumentsCommand = new RelayCommand(ImportDocumentsAsync);
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

            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                await documentService.DeleteDocumentAsync(SelectedDocument);
            }

            RaisePropertyChanged(() => CategoriesEmpty);
            RaisePropertyChanged(() => CategoriesNotEmpty);
        }

        private async void RenameCategoryAsync(Category cat)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                await documentService.RenameCategoryAsync(cat, NewCategoryName);
            }

            RaisePropertyChanged(() => CategoriesEmpty);
            RaisePropertyChanged(() => CategoriesNotEmpty);
        }

        private async void DeleteCategoryAsync(Category cat)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                foreach (var document in cat.Documents.Where(d => !(d is AdDocument))) {
                    await documentService.DeleteDocumentAsync(document);
                }
            }

            RaisePropertyChanged(() => CategoriesEmpty);
            RaisePropertyChanged(() => CategoriesNotEmpty);
        }

        private async void ExportDocumentsAsync()
        {
            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                var status = await licenseService.TryGetLicenseAsync("ExportImportDocuments");
                if (status == LicenseStatus.Unlocked) {
                    var fileTypes = new Dictionary<string, IList<string>> {
                        { translatorService.Translate("archive"), new List<string> { ".zip" } }
                    };
                    var savedFiles = new HashSet<string>();
                    var zipFile = await fileSavePickerService.PickSaveFileAsync("MyDocs.zip", fileTypes);
                    if (zipFile != null) {
                        using (var zipFileStream = await zipFile.OpenWriteAsync())
                        using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create)) {
                            var documents = documentService.Categories.SelectMany(c => c.Documents).Where(d => !(d is AdDocument));

                            var metaInfoEntry = archive.CreateEntry("Documents.xml");
                            using (var metaInfoStream = metaInfoEntry.Open()) {
                                DataContractSerializer serializer = new DataContractSerializer(typeof(IEnumerable<Serializable.Document>), "Documents", "http://mydocs.eggapauli");
                                var serializedDocuments = documents.Select(d =>
                                {
                                    var files = d.Photos.Select(p => String.Format("{0}{1}", p.Title, Path.GetExtension(p.File.Name))).Distinct();
                                    return new Serializable.Document(d.Id, d.Category, d.Tags, d.DateAdded, d.Lifespan, d.HasLimitedLifespan, files);
                                });
                                serializer.WriteObject(metaInfoStream, serializedDocuments);
                            }

                            foreach (var document in documents) {
                                foreach (var photo in document.Photos) {
                                    var fileName = String.Format("{0}{1}", photo.Title, Path.GetExtension(photo.File.Name));
                                    var path = Path.Combine(document.GetHumanReadableDescription(), fileName);
                                    if (savedFiles.Add(path)) {
                                        var entry = archive.CreateEntry(path);
                                        using (var photoReader = await photo.File.OpenReadAsync())
                                        using (var entryStream = entry.Open()) {
                                            await photoReader.CopyToAsync(entryStream);
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
        }

        // TODO set options for import (overwrite existing documents, delete documents before importing, ...)
        private async void ImportDocumentsAsync()
        {
            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                var status = await licenseService.TryGetLicenseAsync("ExportImportDocuments");
                if (status == LicenseStatus.Unlocked) {
                    var zipFile = await fileOpenPickerService.PickOpenFileAsync(new List<string> { ".zip" });
                    if (zipFile == null) {
                        return;
                    }
                    try {
                        using (new TemporaryState(() => IsLoaded = false, () => IsLoaded = true))
                        using (var zipFileStream = await zipFile.OpenReadAsync())
                        using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Read)) {
                            var metaInfoEntry = archive.GetEntry("Documents.xml");
                            if (metaInfoEntry == null) {
                                await uiService.ShowErrorAsync("documentDescriptionFileNotFound");
                                return;
                            }
                            using (var metaInfoStream = metaInfoEntry.Open()) {
                                DataContractSerializer serializer = new DataContractSerializer(typeof(IEnumerable<Serializable.Document>), "Documents", "http://mydocs.eggapauli");
                                var serializedDocuments = (IEnumerable<Serializable.Document>)serializer.ReadObject(metaInfoStream);
                                foreach (var serializedDocument in serializedDocuments) {
                                    var document = await DeserializeDocumentAsync(archive, serializedDocument);
                                    await documentService.SaveDocumentAsync(document);

                                    var originalDocument = Categories.SelectMany(c => c.Documents).SingleOrDefault(d => d.Id == document.Id);
                                    if (originalDocument != null) {
                                        documentService.DetachDocument(originalDocument);
                                    }
                                    Category category = documentService.GetCategoryByName(document.Category);
                                    category.Documents.Add(document);
                                }
                            }
                        }
                        await uiService.ShowNotificationAsync("importFinished");
                    }
                    catch (Exception ex) {
                        // TODO see which exceptions could occur (when Documents.xml is invalid, photos not found, etc.)
                    }
                }
                else if (status == LicenseStatus.Locked) {
                    await uiService.ShowErrorAsync("importLocked");
                }
                else if (status == LicenseStatus.Error) {
                    await uiService.ShowErrorAsync("importUnlockError");
                }
            }
        }

        private async Task<Document> DeserializeDocumentAsync(ZipArchive archive, Serializable.Document document)
        {
            var photos = new List<Photo>();
            foreach (var fileName in document.Files) {
                photos.AddRange(await DeserializePhotosAsync(archive, document, fileName));
            }
            return new Document(document.Id, document.Category, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags, photos);
        }

        private async Task<IEnumerable<Photo>> DeserializePhotosAsync(ZipArchive archive, Serializable.Document document, string fileName)
        {
            var path = String.Format("{0}/{1}", document.GetHumanReadableDescription(), fileName);
            var dirEntry = archive.GetEntry(document.GetHumanReadableDescription());
            var entry = archive.GetEntry(path);
            var photoFileName = String.Format("{0}{1}", Path.GetRandomFileName(), Path.GetExtension(fileName));
            var photoFile = await settingsService.PhotoFolder.CreateFileAsync(photoFileName);
            using (var entryStream = entry.Open())
            using (var photoWriter = await photoFile.OpenWriteAsync()) {
                await entryStream.CopyToAsync(photoWriter);
            }

            var title = Path.GetFileNameWithoutExtension(fileName);
            if (Path.GetExtension(photoFile.Name).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase)) {
                var pages = await pdfService.ExtractPages(photoFile);
                return pages.Select(p => new Photo(title, photoFile, p));
            }
            else {
                return new List<Photo> { new Photo(title, photoFile) };
            }
        }

        #endregion
    }
}