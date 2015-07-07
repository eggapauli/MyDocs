using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyDocs.Common.ViewModel
{
    public class EditDocumentViewModel : ReactiveObject, ICanBeBusy, IDisposable
    {
        private readonly IDocumentService documentService;
        private readonly INavigationService navigator;
        private readonly IUserInterfaceService uiService;
        private readonly ICameraService cameraService;
        private readonly IFileOpenPickerService filePicker;
        private readonly ISettingsService settingsService;
        private readonly IPageExtractorService pageExtractor;

        #region Properties

        private readonly ObservableAsPropertyHelper<IImmutableList<string>> categoryNames;
        private readonly ObservableAsPropertyHelper<bool> showNewCategoryInput;
        private bool showNewCategoryInputValue;
        private string useCategoryName;
        private string newCategoryName;
        private Document originalDocument;
        private Document editingDocument;
        private Photo selectedPhoto;
        private bool isBusy;
        private IDisposable disposables;

        public IImmutableList<string> CategoryNames
        {
            get { return categoryNames.Value; }
        }

        public bool ShowNewCategoryInputValue
        {
            get { return showNewCategoryInputValue; }
            set { this.RaiseAndSetIfChanged(ref showNewCategoryInputValue, value); }
        }

        public bool ShowNewCategoryInput
        {
            get { return showNewCategoryInput.Value; }
        }

        private readonly ObservableAsPropertyHelper<bool> showUseCategoryInput;
        public bool ShowUseCategoryInput
        {
            get { return showUseCategoryInput.Value; }
        }

        private ObservableAsPropertyHelper<bool> hasCategories;

        public bool HasCategories
        {
            get { return hasCategories.Value; }
        }

        public string UseCategoryName
        {
            get { return useCategoryName; }
            set { this.RaiseAndSetIfChanged(ref useCategoryName, value); }
        }

        public string NewCategoryName
        {
            get { return newCategoryName; }
            set { this.RaiseAndSetIfChanged(ref newCategoryName, value); }
        }

        public Document EditingDocument
        {
            get { return editingDocument; }
            set
            {
                if (editingDocument != value)
                {
                    editingDocument = value != null ? value.Clone() : null;
                    originalDocument = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public Guid EditingDocumentId
        {
            set
            {
                // TODO this is just ugly
                EditingDocument = null;
                documentService.GetDocumentById(value).ContinueWith(t => {
                    if (t.IsFaulted) {
                        EditingDocument = new Document();
                        uiService.ShowErrorAsync("documentNotFound");
                    }
                    else {
                        EditingDocument = Document.FromLogic(t.Result);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private ObservableAsPropertyHelper<IImmutableList<Photo>> allPhotos;
        public IImmutableList<Photo> AllPhotos
        {
            get { return allPhotos.Value; }
        }

        public Photo SelectedPhoto
        {
            get { return selectedPhoto; }
            set { this.RaiseAndSetIfChanged(ref selectedPhoto, value); }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        #endregion

        public EditDocumentViewModel(
            IDocumentService documentService,
            INavigationService navigator,
            IUserInterfaceService uiService,
            ICameraService cameraService,
            IFileOpenPickerService filePicker,
            ISettingsService settingsService,
            IPageExtractorService pageExtractor)
        {
            this.documentService = documentService;
            this.navigator = navigator;
            this.uiService = uiService;
            this.cameraService = cameraService;
            this.filePicker = filePicker;
            this.settingsService = settingsService;
            this.pageExtractor = pageExtractor;

            CreateCommands();
            CreateDesignTimeData();

            categoryNames = documentService.GetCategoryNames()
                .Select(x => x.ToImmutableList())
                .ToProperty(this, x => x.CategoryNames, ImmutableList<string>.Empty);

            hasCategories = this.WhenAnyValue(x => x.CategoryNames)
                .Select(x => x.Count > 0)
                .ToProperty(this, x => x.HasCategories);

            showNewCategoryInput = this.WhenAnyValue(x => x.ShowNewCategoryInputValue, x => x.HasCategories, (x, y) => !y || x)
                .ToProperty(this, x => x.ShowNewCategoryInput);

            showUseCategoryInput = this.WhenAnyValue(x => x.ShowNewCategoryInput)
                .Select(x => !x)
                .ToProperty(this, x => x.ShowUseCategoryInput);

            var categorySubscription = this.WhenAnyValue(x => x.EditingDocument.Category)
                .Subscribe(x =>
                {
                    NewCategoryName = x;
                    UseCategoryName = x;
                });

            allPhotos = this.WhenAnyValue(x => x.EditingDocument.SubDocuments)
                .Select(subDocs => subDocs
                    .Select(doc => doc
                        .WhenAnyValue(x => x.Photos)
                    )
                    .CombineLatest(x => x
                        .SelectMany(y => y)
                        .ToImmutableList()
                    )
                )
                .Switch()
                .ToProperty(this, x => x.AllPhotos);

            disposables = new CompositeDisposable(categoryNames, showUseCategoryInput, hasCategories, categorySubscription, allPhotos);
        }

        [Conditional("DEBUG")]
        private async void CreateDesignTimeData()
        {
            if (ModeDetector.InDesignMode())
            {
                var docs = await documentService.GetDocuments().Take(1).ToTask();
                EditingDocument = Document.FromLogic(docs.First());
            }
        }

        #region Commands

        public ICommand ShowNewCategoryCommand { get; private set; }
        public ICommand ShowUseCategoryCommand { get; private set; }
        public ICommand AddPhotoFromCameraCommand { get; private set; }
        public ICommand AddPhotoFromFileCommand { get; private set; }
        public ICommand RemovePhotoCommand { get; private set; }
        public ICommand SaveDocumentCommand { get; private set; }

        private void CreateCommands()
        {
            AddPhotoFromCameraCommand = this.CreateAsyncCommand(_ => AddPhotoFromCameraAsync());
            AddPhotoFromFileCommand = this.CreateAsyncCommand(_ => AddPhotoFromFileAsync());
            RemovePhotoCommand = this.CreateCommand(_ => RemovePhoto(), this.WhenAnyValue(x => x.SelectedPhoto).Select(x => x != null));
            SaveDocumentCommand = this.CreateAsyncCommand(_ => SaveDocumentAsync(),
                this.WhenAnyValue(
                    x => x.EditingDocument.Tags,
                    x => x.EditingDocument.SubDocuments,
                    x => x.ShowNewCategoryInput,
                    x => x.NewCategoryName,
                    x => x.UseCategoryName,
                    (tags, subDocs, showNewCategoryInput, newCategoryName, useCategoryName) =>
                    {
                        var test = this.ShowNewCategoryInput;
                        var newCategoryInputOk = !showNewCategoryInput || !string.IsNullOrWhiteSpace(newCategoryName);
                        var useCategoryInputOk = showNewCategoryInput || !string.IsNullOrWhiteSpace(useCategoryName);
                        return tags.Any()
                            && newCategoryInputOk
                            && useCategoryInputOk
                            && subDocs.Count > 0;
                    }
                )
            );
            ShowNewCategoryCommand = this.CreateCommand(_ => ShowNewCategoryInputValue = true);
            ShowUseCategoryCommand = this.CreateCommand(_ => ShowNewCategoryInputValue = false);
        }

        private async Task SaveDocumentAsync()
        {
            EditingDocument.Category = ShowNewCategoryInput ? NewCategoryName : UseCategoryName;

            await documentService.SaveDocumentAsync(EditingDocument.ToLogic());

            // Delete removed photos
            if (originalDocument != null) {
                var oldPhotos = originalDocument.SubDocuments.SelectMany(d => d.Photos);
                var newPhotos = EditingDocument.SubDocuments.SelectMany(d => d.Photos);
                var deletedPhotos = oldPhotos.Where(p => !newPhotos.Contains(p));
                await documentService.RemovePhotosAsync(deletedPhotos.Select(p => p.ToLogic()));
            }

            var doc = EditingDocument;
            EditingDocument = null;
            originalDocument = doc;

            navigator.Navigate<IMainPage>(originalDocument.Id);
        }

        private async Task AddPhotoFromCameraAsync()
        {
            try
            {
                var photo = await cameraService.GetPhotoForDocumentAsync(EditingDocument);
                if (photo != null)
                {
                    EditingDocument.AddSubDocument(new SubDocument(photo.File, new[] { photo }));
                }
            }
            // TODO refine
            catch (Exception)
            {
                // TODO translate
                await uiService.ShowErrorAsync("addPhotoFromCameraError");
            }
        }

        private async Task AddPhotoFromFileAsync()
        {
            var files = await filePicker.PickFilesForDocumentAsync(EditingDocument);

            var error = false;
            foreach (var file in files) {
                try {
                    var pages =
                        pageExtractor.SupportsExtension(Path.GetExtension(file.Name)) ?
                        await pageExtractor.ExtractPages(file, EditingDocument.ToLogic()) :
                        null;
                    EditingDocument.AddSubDocument(new SubDocument(file, pages.Select(Photo.FromLogic)));
                }
                catch (Exception) {
                    error = true;
                }
            }

            if (error) {
                await uiService.ShowErrorAsync("fileLoadError");
            }
        }

        private void RemovePhoto()
        {
            EditingDocument.RemovePhoto(SelectedPhoto);
            SelectedPhoto = null;
        }

        public void Dispose()
        {
            disposables.Dispose();
        }

        #endregion
    }
}
