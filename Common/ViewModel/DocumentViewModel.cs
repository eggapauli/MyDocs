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
using MyDocs.Common.Contract.Storage;
using System.Collections.ObjectModel;

namespace MyDocs.Common.ViewModel
{
    public class DocumentViewModel : ViewModelBase
    {
        private readonly IDocumentService documentService;
        private readonly INavigationService navigationService;
        private readonly IUserInterfaceService uiService;
        private readonly ILicenseService licenseService;
        private readonly IExportDocumentService exportDocumentService;
        private readonly IImportDocumentService importDocumentService;

        #region Properties

        private Document selectedDocument;
        private string newCategoryName;
        private bool inCategoryEditMode = false;
        private bool inZoomedInView = true;
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
            IExportDocumentService exportDocumentService,
            IImportDocumentService importDocumentService)
        {
            this.documentService = documentService;
            this.navigationService = navigationService;
            this.uiService = uiService;
            this.licenseService = licenseService;
            this.exportDocumentService = exportDocumentService;
            this.importDocumentService = importDocumentService;

            documentService.Documents.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(() => Categories);
                RaisePropertyChanged(() => CategoriesEmpty);
            };

            CreateCommands();
            CreateDesignTimeData();
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
            using (new TemporaryState(() => IsLoading = true, () => IsLoading = false)) {
                await documentService.LoadDocumentsAsync();
                if (selectedDocumentId.HasValue) {
                    SelectedDocument = await documentService.GetDocumentById(selectedDocumentId.Value);
                }
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
                string error = null;
                try {
                    await licenseService.Unlock("ExportImportDocuments");
                    await exportDocumentService.ExportDocuments(new ReadOnlyCollection<Document>(documentService.Documents));
                }
                catch (LicenseStatusException e) {
                    if (e.LicenseStatus == LicenseStatus.Locked) {
                        error = "exportLocked";
                    }
                    else if (e.LicenseStatus == LicenseStatus.Error) {
                        error = "exportUnlockError";
                    }
                }
                catch (Exception) {
                    // TODO refine errors
                    error = "exportError";
                }
                if (error != null) {
                    await uiService.ShowErrorAsync(error);
                }
                else {
                    await uiService.ShowNotificationAsync("exportFinished");
                }
            }
        }

        // TODO set options for import (overwrite existing documents, delete documents before importing, ...)
        private async void ImportDocumentsAsync()
        {
            using (new TemporaryState(() => IsBusy = true, () => IsBusy = false)) {
                string error = null;
                try {
                    await licenseService.Unlock("ExportImportDocuments");
                    await importDocumentService.ImportDocuments();
                }
                catch (LicenseStatusException e) {
                    if (e.LicenseStatus == LicenseStatus.Locked) {
                        error = "importLocked";
                    }
                    else if (e.LicenseStatus == LicenseStatus.Error) {
                        error = "importUnlockError";
                    }
                }
                catch (ImportManifestNotFoundException) {
                    error = "documentDescriptionFileNotFound";
                }
                catch (Exception) {
                    // TODO refine errors
                    error = "importError";
                }
                if (error != null) {
                    await uiService.ShowErrorAsync(error);
                }
                else {
                    await uiService.ShowNotificationAsync("importFinished");
                }
            }
        }

        #endregion
    }
}