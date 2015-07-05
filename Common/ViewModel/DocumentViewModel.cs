using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Messages;
using View = MyDocs.Common.Model.View;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Immutable;
using MyDocs.Common.Model;
using ReactiveUI;
using System.Reactive.Linq;
using System.Windows.Input;
using Splat;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace MyDocs.Common.ViewModel
{
    public class DocumentViewModel : ReactiveObject, ICanBeBusy, IDisposable
    {
        private readonly IDocumentService documentService;
        private readonly INavigationService navigationService;
        private readonly IUserInterfaceService uiService;
        private readonly ILicenseService licenseService;
        private readonly IExportDocumentService exportDocumentService;
        private readonly IImportDocumentService importDocumentService;

        #region Properties

        private readonly ObservableAsPropertyHelper<IImmutableList<View.Category>> categories;
        private View.Document selectedDocument;
        private string newCategoryName;
        private bool inCategoryEditMode = false;
        private bool inZoomedInView = true;
        private readonly ObservableAsPropertyHelper<bool> isLoading;
        private bool isBusy = false;
        private ISubject<CloseFlyoutsMessage> closeFlyoutsMessages =
            new Subject<CloseFlyoutsMessage>();

        public bool InZoomedInView
        {
            get { return inZoomedInView; }
            set { this.RaiseAndSetIfChanged(ref inZoomedInView, value); }
        }

        public IImmutableList<View.Category> Categories
        {
            get { return categories.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> categoriesEmpty;
        public bool CategoriesEmpty
        {
            get { return categoriesEmpty.Value; }
        }

        public View.Document SelectedDocument
        {
            get { return selectedDocument; }
            set { this.RaiseAndSetIfChanged(ref selectedDocument, value); }
        }

        private readonly ObservableAsPropertyHelper<bool> hasSelectedDocument;
        private CompositeDisposable disposables;

        public bool HasSelectedDocument
        {
            get { return hasSelectedDocument.Value; }
        }

        public string NewCategoryName
        {
            get { return newCategoryName; }
            set { this.RaiseAndSetIfChanged(ref newCategoryName, value); }
        }

        public bool InEditCategoryMode
        {
            get { return inCategoryEditMode; }
            set { this.RaiseAndSetIfChanged(ref inCategoryEditMode, value); }
        }

        public bool IsLoading
        {
            get { return isLoading.Value; }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        public IObservable<CloseFlyoutsMessage> CloseFlyoutsMessages
        {
            get { return closeFlyoutsMessages.AsObservable(); }
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

            var viewCategories = documentService.GetDocuments()
               .Select(docs => docs
                    .GroupBy(d => d.Category)
                    .Select(g => new View.Category(g.Key, g.Select(View.Document.FromLogic)))
                    .ToImmutableList()
                );

            categories = viewCategories
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.Categories, ImmutableList<View.Category>.Empty);

            isLoading = documentService.GetDocuments()
                .Take(1)
                .Select(_ => false)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.IsLoading, true);

            categoriesEmpty = viewCategories.Select(c => c.Count == 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.CategoriesEmpty);

            hasSelectedDocument = this.WhenAnyValue(x => x.SelectedDocument)
                .Select(x => x != null)
                .ToProperty(this, x => x.HasSelectedDocument);

            disposables = new CompositeDisposable(categories, isLoading, categoriesEmpty, hasSelectedDocument);

            CreateCommands();
            CreateDesignTimeData();
        }

        [Conditional("DEBUG")]
        private async void CreateDesignTimeData()
        {
            if (ModeDetector.InDesignMode())
            {
                var docs = await documentService.GetDocuments().Take(1).ToTask();
                var doc = docs.First(d => d.Tags.Count > 2);
                SelectedDocument = View.Document.FromLogic(doc);
            }
        }

        public void TrySelectDocument(Guid selectedDocumentId)
        {
            SelectedDocument = Categories
                .SelectMany(c => c.Documents)
                .SingleOrDefault(d => d.Id == selectedDocumentId);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        #region Commands

        public ICommand AddDocumentCommand { get; private set; }
        public ICommand EditDocumentCommand { get; private set; }
        public ICommand DeleteDocumentCommand { get; private set; }
        public ICommand ShowDocumentCommand { get; private set; }
        public ICommand RenameCategoryCommand { get; private set; }
        public ICommand DeleteCategoryCommand { get; private set; }
        public ICommand NavigateToSearchPageCommand { get; private set; }
        public ICommand ExportDocumentsCommand { get; private set; }
        public ICommand ImportDocumentsCommand { get; private set; }

        private void CreateCommands()
        {
            AddDocumentCommand = this.CreateCommand(_ => AddDocument());
            EditDocumentCommand = this.CreateCommand(_ => EditDocument(), this.WhenAnyValue(x => x.HasSelectedDocument));
            DeleteDocumentCommand = this.CreateAsyncCommand(_ => DeleteDocumentAsync(), this.WhenAnyValue(x => x.HasSelectedDocument));
            ShowDocumentCommand = this.CreateCommand(o => navigationService.Navigate<IShowDocumentPage>(((View.Document)o).Id));

            RenameCategoryCommand = this.CreateAsyncCommand(o => RenameCategoryAsync((View.Category)o), this.WhenAnyValue(x => x.NewCategoryName).Select(x => !string.IsNullOrWhiteSpace(x)));
            DeleteCategoryCommand = this.CreateAsyncCommand(o => DeleteCategoryAsync((View.Category)o));

            NavigateToSearchPageCommand = this.CreateCommand(_ => navigationService.Navigate<ISearchPage>());

            ExportDocumentsCommand = this.CreateAsyncCommand(_ => ExportDocumentsAsync());
            ImportDocumentsCommand = this.CreateAsyncCommand(_ => ImportDocumentsAsync());
        }

        private void AddDocument()
        {
            navigationService.Navigate<IEditDocumentPage>();
        }

        private void EditDocument()
        {
            navigationService.Navigate<IEditDocumentPage>(selectedDocument.Id);
        }

        private async Task DeleteDocumentAsync()
        {
            closeFlyoutsMessages.OnNext(new CloseFlyoutsMessage());

            await documentService.DeleteDocumentAsync(selectedDocument.ToLogic());
            foreach (var category in Categories.Where(c => c.Documents.Contains(selectedDocument)))
            {
                category.Documents = category.Documents.Remove(selectedDocument);
            }
            SelectedDocument = null;
        }

        private async Task RenameCategoryAsync(View.Category category)
        {
            closeFlyoutsMessages.OnNext(new CloseFlyoutsMessage());

            await documentService.RenameCategoryAsync(category.Name, newCategoryName);
            NewCategoryName = null;
        }

        private async Task DeleteCategoryAsync(View.Category category)
        {
            closeFlyoutsMessages.OnNext(new CloseFlyoutsMessage());

            await documentService.DeleteCategoryAsync(category.Name);
        }

        private async Task ExportDocumentsAsync()
        {
            string error = null;
            try
            {
                await licenseService.Unlock("ExportImportDocuments");
                await exportDocumentService.ExportDocuments();
            }
            catch (LicenseStatusException e)
            {
                if (e.LicenseStatus == LicenseStatus.Locked)
                {
                    error = "exportLocked";
                }
                else if (e.LicenseStatus == LicenseStatus.Error)
                {
                    error = "exportUnlockError";
                }
            }
            catch (Exception)
            {
                // TODO refine errors
                error = "exportError";
            }
            if (error != null)
            {
                await uiService.ShowErrorAsync(error);
            }
            else
            {
                await uiService.ShowNotificationAsync("exportFinished");
            }
        }

        // TODO set options for import (overwrite existing documents, delete documents before importing, ...)
        private async Task ImportDocumentsAsync()
        {
            string error = null;
            try
            {
                await licenseService.Unlock("ExportImportDocuments");
                await importDocumentService.ImportDocuments();
            }
            catch (LicenseStatusException e)
            {
                if (e.LicenseStatus == LicenseStatus.Locked)
                {
                    error = "importLocked";
                }
                else if (e.LicenseStatus == LicenseStatus.Error)
                {
                    error = "importUnlockError";
                }
            }
            catch (ImportManifestNotFoundException)
            {
                error = "documentDescriptionFileNotFound";
            }
            catch (Exception)
            {
                // TODO refine errors
                error = "importError";
            }
            if (error != null)
            {
                await uiService.ShowErrorAsync(error);
            }
            else
            {
                await uiService.ShowNotificationAsync("importFinished");
            }
        }

        #endregion
    }
}