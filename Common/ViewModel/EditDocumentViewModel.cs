using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyDocs.Common.ViewModel
{
    public class EditDocumentViewModel : ViewModelBase
    {
        private readonly IDocumentService documentService;
        private readonly INavigationService navigator;
        private readonly IUserInterfaceService uiService;
        private readonly ICameraService cameraService;
        private readonly IFileOpenPickerService filePicker;
        private readonly ISettingsService settingsService;
        private readonly IPageExtractor pageExtractor;

        #region Properties

        private bool showNewCategoryInput;
        private string useCategoryName;
        private string newCategoryName;
        private Document originalDocument;
        private Document editingDocument;
        private Photo selectedPhoto;
        private bool isBusy;

        public IEnumerable<string> CategoryNames
        {
            get { return documentService.GetCategoryNames(); }
        }

        public bool ShowNewCategoryInput
        {
            get { return !HasCategories || showNewCategoryInput; }
            set
            {
                if (Set(ref showNewCategoryInput, value)) {
                    RaisePropertyChanged(() => ShowUseCategoryInput);
                    SaveDocumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool ShowUseCategoryInput
        {
            get { return !ShowNewCategoryInput; }
            set { ShowNewCategoryInput = !value; }
        }

        public bool HasCategories
        {
            get { return CategoryNames.Any(); }
        }

        public string UseCategoryName
        {
            get { return useCategoryName; }
            set
            {
                if (Set(ref useCategoryName, value)) {
                    SaveDocumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string NewCategoryName
        {
            get { return newCategoryName; }
            set
            {
                if (Set(ref newCategoryName, value)) {
                    SaveDocumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public Document EditingDocument
        {
            get { return editingDocument; }
            set
            {
                if (editingDocument != null) {
                    editingDocument.PropertyChanged -= EditingDocumentChangedHandler;
                }
                if (editingDocument != value) {
                    if (value != null) {
                        editingDocument = value.Clone();
                        NewCategoryName = editingDocument.Category;
                        UseCategoryName = editingDocument.Category;
                        editingDocument.PropertyChanged += EditingDocumentChangedHandler;
                    }
                    else {
                        editingDocument = null;
                    }
                    originalDocument = value;
                    RaisePropertyChanged();
                    SaveDocumentCommand.RaiseCanExecuteChanged();
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

        private void EditingDocumentChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            // TODO make strongly typed
            if (e.PropertyName == "Tags" || e.PropertyName == "Category" || e.PropertyName == "SubDocuments") {
                SaveDocumentCommand.RaiseCanExecuteChanged();
            }
        }

        public Photo SelectedPhoto
        {
            get { return selectedPhoto; }
            set
            {
                if (Set(ref selectedPhoto, value)) {
                    RemovePhotoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set { Set(ref isBusy, value); }
        }

        #endregion

        public EditDocumentViewModel(
            IDocumentService documentService,
            INavigationService navigator,
            IUserInterfaceService uiService,
            ICameraService cameraService,
            IFileOpenPickerService filePicker,
            ISettingsService settingsService,
            IPageExtractor pageExtractor)
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

            ShowUseCategoryInput = documentService.GetCategoryNames().Any();
        }

        [Conditional("DEBUG")]
        private void CreateDesignTimeData()
        {
            if (IsInDesignMode) {
                documentService.LoadAsync().ContinueWith(t => {
                    EditingDocument = Document.FromLogic(t.Result.First());
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public async Task LoadAsync()
        {
            using (SetBusy()) {
                await documentService.LoadAsync();
            }
        }

        #region Commands

        public RelayCommand ShowNewCategoryCommand { get; private set; }
        public RelayCommand ShowUseCategoryCommand { get; private set; }
        public RelayCommand AddPhotoFromCameraCommand { get; private set; }
        public RelayCommand AddPhotoFromFileCommand { get; private set; }
        public RelayCommand RemovePhotoCommand { get; private set; }
        public RelayCommand SaveDocumentCommand { get; private set; }

        private void CreateCommands()
        {
            AddPhotoFromCameraCommand = new RelayCommand(AddPhotoFromCameraAsync);
            AddPhotoFromFileCommand = new RelayCommand(AddPhotoFromFileAsync);
            RemovePhotoCommand = new RelayCommand(RemovePhoto, () => SelectedPhoto != null);
            SaveDocumentCommand = new RelayCommand(SaveDocumentAsync, () =>
                EditingDocument != null
                && EditingDocument.Tags.Any()
                && (ShowNewCategoryInput && !String.IsNullOrWhiteSpace(NewCategoryName)
                    || ShowUseCategoryInput && !String.IsNullOrWhiteSpace(UseCategoryName))
                && EditingDocument.SubDocuments.Count > 0);
            ShowNewCategoryCommand = new RelayCommand(() => { ShowNewCategoryInput = true; });
            ShowUseCategoryCommand = new RelayCommand(() => { ShowNewCategoryInput = false; });
        }

        private async void SaveDocumentAsync()
        {
            EditingDocument.Category = ShowNewCategoryInput ? NewCategoryName : UseCategoryName;

            using (SetBusy()) {
                await documentService.SaveDocumentAsync(EditingDocument.ToLogic());

                // Delete removed photos
                if (originalDocument != null) {
                    var oldPhotos = originalDocument.SubDocuments.SelectMany(d => d.Photos);
                    var newPhotos = EditingDocument.SubDocuments.SelectMany(d => d.Photos);
                    var deletedPhotos = oldPhotos.Where(p => !newPhotos.Contains(p));
                    await documentService.RemovePhotosAsync(deletedPhotos.Select(p => p.ToLogic()));
                }
            }

            var doc = EditingDocument;
            EditingDocument = null;
            originalDocument = doc;

            navigator.Navigate<IMainPage>(originalDocument.Id);
        }

        private async void AddPhotoFromCameraAsync()
        {
            var photo = await cameraService.GetPhotoForDocumentAsync(EditingDocument);
            if (photo != null) {
                EditingDocument.AddSubDocument(new SubDocument(photo.File, new[] { photo }));
            }
        }

        private async void AddPhotoFromFileAsync()
        {
            var files = await filePicker.PickFilesForDocumentAsync(EditingDocument);

            using (SetBusy()) {
                var error = false;
                foreach (var file in files) {
                    try {
                        var pages =
                            pageExtractor.SupportedExtensions.Contains(Path.GetExtension(file.Name)) ?
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
        }

        private void RemovePhoto()
        {
            EditingDocument.RemovePhoto(SelectedPhoto);
        }

        #endregion

        private IDisposable SetBusy()
        {
            return new TemporaryState(() => IsBusy = true, () => IsBusy = false);
        }
    }
}
