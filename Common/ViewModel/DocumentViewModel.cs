using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Messages;
using View = MyDocs.Common.Model.View;
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
using System.Collections.Immutable;
using MyDocs.Common.Model;

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

        private IImmutableList<View.Category> categories = ImmutableList<View.Category>.Empty;
        private View.Document selectedDocument;
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

        public IImmutableList<View.Category> Categories
        {
            get { return categories; }
            set { Set(ref categories, value); }
        }

        public bool CategoriesEmpty
        {
            get { return !IsBusy && categories.Count == 0; }
        }

        public View.Document SelectedDocument
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

            documentService.Changed += (s, e) =>
            {
                // TODO check `e` and update collection

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
                documentService.LoadAsync().ContinueWith(t =>
                {
                    var doc = t.Result
                        .First(d => d.Tags.Count > 2);
                    SelectedDocument = View.Document.FromLogic(doc);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public async Task LoadAsync(Guid? selectedDocumentId = null)
        {
            using (new TemporaryState(() => IsLoading = true, () => IsLoading = false)) {
                var documents = await documentService.LoadAsync();
                Categories = documents
                    .GroupBy(d => d.Category)
                    .Select(g => new View.Category(g.Key, g.Select(View.Document.FromLogic)))
                    .ToImmutableList();
                if (selectedDocumentId.HasValue) {
                    var doc = await documentService.GetDocumentById(selectedDocumentId.Value);
                    SelectedDocument = View.Document.FromLogic(doc);
                }
            }
        }

        #region Commands

        public RelayCommand AddDocumentCommand { get; private set; }
        public RelayCommand EditDocumentCommand { get; private set; }
        public RelayCommand DeleteDocumentCommand { get; private set; }
        public RelayCommand<View.Document> ShowDocumentCommand { get; private set; }
        public RelayCommand<View.Category> RenameCategoryCommand { get; private set; }
        public RelayCommand<View.Category> DeleteCategoryCommand { get; private set; }
        public RelayCommand NavigateToSearchPageCommand { get; private set; }
        public RelayCommand ExportDocumentsCommand { get; private set; }
        public RelayCommand ImportDocumentsCommand { get; private set; }

        private void CreateCommands()
        {
            AddDocumentCommand = new RelayCommand(AddDocument);
            EditDocumentCommand = new RelayCommand(EditDocument, () => SelectedDocument != null);
            DeleteDocumentCommand = new RelayCommand(DeleteDocumentAsync, () => SelectedDocument != null);
            ShowDocumentCommand = new RelayCommand<View.Document>(doc => navigationService.Navigate<IShowDocumentPage>(doc.Id), doc => doc != null);

            RenameCategoryCommand = new RelayCommand<View.Category>(RenameCategoryAsync, _ => !String.IsNullOrWhiteSpace(newCategoryName));
            DeleteCategoryCommand = new RelayCommand<View.Category>(DeleteCategoryAsync);

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

            using (SetBusy()) {
                await documentService.DeleteDocumentAsync(selectedDocument.ToLogic());
            }
        }

        private async void RenameCategoryAsync(View.Category category)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            using (SetBusy()) {
                await documentService.RenameCategoryAsync(category.Name, newCategoryName);
            }
            NewCategoryName = null;
        }

        private async void DeleteCategoryAsync(View.Category category)
        {
            MessengerInstance.Send(new CloseFlyoutsMessage());

            using (SetBusy()) {
                await documentService.DeleteCategoryAsync(category.Name);
            }
        }

        private async void ExportDocumentsAsync()
        {
            using (SetBusy()) {
                string error = null;
                try {
                    await licenseService.Unlock("ExportImportDocuments");
                    await exportDocumentService.ExportDocuments();
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
            using (SetBusy()) {
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

        public IDisposable SetBusy()
        {
            return new TemporaryState(() => IsBusy = true, () => IsBusy = false);
        }

        #endregion
    }
}