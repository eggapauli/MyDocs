using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        private string newCategoryName;
        private bool inCategoryEditMode = false;
        private bool inZoomedInView;
        private bool isLoading = false;
        private bool isBusy = false;

        public bool InZoomedInView
        {
            get { return inZoomedInView; }
            set { Set(ref inZoomedInView, value); }
        }

        public IEnumerable<Category> Categories
        {
            get { return documentService.Documents.OrderBy(d => d.Category).ThenByDescending(d => d.DateAdded).ThenBy(d => d.Id).SelectMany(DocumentsAndAds).GroupBy(d => d.Category).Select(g => new Category(g.Key, g)); }
        }

        public IEnumerable<IDocument> DocumentsAndAds(Document document, int index)
        {
            if (index > 0 && index % 5 == 0) {
                yield return new AdDocument(document.Category);
            }
            yield return document;
        }

        public bool CategoriesEmpty
        {
            get { return !IsBusy && documentService.Documents.Count == 0; }
        }

        public IDocument SelectedDocument
        {
            get { return selectedDocument; }
            set
            {
                if (Set(ref selectedDocument, value as Document)) {
                    RaisePropertyChanged(() => HasSelectedDocument);

                    DeleteDocumentCommand.RaiseCanExecuteChanged();
                    EditDocumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool HasSelectedDocument
        {
            get { return SelectedDocument != null; }
        }

        public string NewCategoryName
        {
            get { return newCategoryName; }
            set
            {
                if (Set(ref newCategoryName, value)) {
                    RenameCategoryCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool InEditCategoryMode
        {
            get { return inCategoryEditMode; }
            set { Set(ref inCategoryEditMode, value); }
        }

        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(ref isLoading, value); }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (Set(ref isBusy, value)) {
                    RaisePropertyChanged(() => CategoriesEmpty);
                }
            }
        }

        #endregion

        public DocumentViewModel(
            IDocumentService documentService,
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

            documentService.Documents.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(() => Categories);
                RaisePropertyChanged(() => CategoriesEmpty);
            };

            CreateCommands();
            CreateDesignTimeData();

            InZoomedInView = true;
        }

        [Conditional("DEBUG")]
        private void CreateDesignTimeData()
        {
            if (IsInDesignMode) {
                documentService.LoadDocumentsAsync().ContinueWith(t =>
                {
                    SelectedDocument = documentService.Documents.First(d => d.Tags.Count > 2);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public async Task LoadAsync(Guid? selectedDocumentId = null)
        {
            bool error = false;
            try {
                using (new TemporaryState(() => IsLoading = true, () => IsLoading = false)) {
                    await documentService.LoadDocumentsAsync();
                    if (selectedDocumentId.HasValue) {
                        SelectedDocument = await documentService.GetDocumentById(selectedDocumentId.Value);
                    }
                }
            }
            catch (Exception ex) {
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
            EditDocumentCommand = new RelayCommand(EditDocument, () => SelectedDocument != null);
            DeleteDocumentCommand = new RelayCommand(DeleteDocumentAsync, () => SelectedDocument != null);
            ShowDocumentCommand = new RelayCommand<Document>(doc => navigationService.Navigate<IShowDocumentPage>(doc.Id), doc => doc != null);

            RenameCategoryCommand = new RelayCommand<Category>(RenameCategoryAsync, _ => !String.IsNullOrWhiteSpace(newCategoryName));
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
            navigationService.Navigate<IEditDocumentPage>(selectedDocument.Id);
        }

        private async void DeleteDocumentAsync()
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                await documentService.DeleteDocumentAsync(selectedDocument);
            }
        }

        private async void RenameCategoryAsync(Category category)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                await documentService.RenameCategoryAsync(category.Name, newCategoryName);
            }
            NewCategoryName = null;
        }

        private async void DeleteCategoryAsync(Category category)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                foreach (var document in documentService.Documents.Where(d => d.Category == category.Name).ToList()) {
                    await documentService.DeleteDocumentAsync(document);
                }
            }
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
                            var metaInfoEntry = archive.CreateEntry("Documents.xml");
                            using (var metaInfoStream = metaInfoEntry.Open()) {
                                DataContractSerializer serializer = new DataContractSerializer(typeof(IEnumerable<Serializable.Document>), "Documents", "http://mydocs.eggapauli");
                                var serializedDocuments = documentService.Documents.Select(d =>
                                {
                                    var files = d.Photos.Select(p => String.Format("{0}{1}", p.Title, Path.GetExtension(p.File.Name))).Distinct();
                                    return new Serializable.Document(d.Id, d.Category, d.Tags, d.DateAdded, d.Lifespan, d.HasLimitedLifespan, files);
                                });
                                serializer.WriteObject(metaInfoStream, serializedDocuments);
                            }

                            foreach (var document in documentService.Documents) {
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
                photos.Add(await DeserializePhotosAsync(archive, document, fileName));
            }
            return new Document(document.Id, document.Category, document.DateAdded, document.Lifespan, document.HasLimitedLifespan, document.Tags, photos);
        }

        private async Task<Photo> DeserializePhotosAsync(ZipArchive archive, Serializable.Document document, string fileName)
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
                return new Photo(title, photoFile, pages);
            }
            else {
                return new Photo(title, photoFile);
            }
        }

        #endregion
    }
}