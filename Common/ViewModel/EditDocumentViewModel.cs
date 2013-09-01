using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
                if (showNewCategoryInput != value) {
                    showNewCategoryInput = value;
                    RaisePropertyChanged(() => ShowNewCategoryInput);
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
                if (useCategoryName != value) {
                    useCategoryName = value;
                    RaisePropertyChanged(() => UseCategoryName);
                    SaveDocumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string NewCategoryName
        {
            get { return newCategoryName; }
            set
            {
                if (newCategoryName != value) {
                    newCategoryName = value;
                    RaisePropertyChanged(() => NewCategoryName);
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
                        editingDocument.Photos.CollectionChanged += EditingDocumentPhotoCollectionChanged;
                    }
                    else {
                        editingDocument = null;
                    }
                    originalDocument = value;
                    RaisePropertyChanged(() => EditingDocument);
                    SaveDocumentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public Guid EditingDocumentId
        {
            set
            {
                EditingDocument = null;
                documentService.GetDocumentById(value).ContinueWith(t =>
                {
                    if (t.IsFaulted) {
                        uiService.ShowErrorAsync("documentNotFound");
                        EditingDocument = new Document();
                    }
                    else {
                        EditingDocument = t.Result;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void EditingDocumentChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tags" || e.PropertyName == "Category") {
                SaveDocumentCommand.RaiseCanExecuteChanged();
            }
        }

        private void EditingDocumentPhotoCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveDocumentCommand.RaiseCanExecuteChanged();
        }

        public Photo SelectedPhoto
        {
            get { return selectedPhoto; }
            set
            {
                if (selectedPhoto != value) {
                    selectedPhoto = value;
                    RaisePropertyChanged(() => SelectedPhoto);
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
            IFileOpenPickerService filePicker)
        {
            this.documentService = documentService;
            this.navigator = navigator;
            this.uiService = uiService;
            this.cameraService = cameraService;
            this.filePicker = filePicker;

            CreateCommands();
            CreateDesignTimeData();

            ShowUseCategoryInput = documentService.GetCategoryNames().Any();
        }

        [Conditional("DEBUG")]
        private void CreateDesignTimeData()
        {
            if (IsInDesignMode) {
                documentService.LoadCategoriesAsync().ContinueWith(t =>
                {
                    EditingDocument = documentService.Categories.First().Documents.First();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public async Task LoadAsync()
        {
            await documentService.LoadCategoriesAsync();
        }

        #region Commands

        public RelayCommand ShowNewCategoryCommand { get; set; }
        public RelayCommand ShowUseCategoryCommand { get; set; }
        public RelayCommand AddPhotoFromCameraCommand { get; set; }
        public RelayCommand AddPhotoFromFileCommand { get; set; }
        public RelayCommand RemovePhotoCommand { get; set; }
        public RelayCommand SaveDocumentCommand { get; set; }

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
                && EditingDocument.Photos.Count > 0);
            ShowNewCategoryCommand = new RelayCommand(() => { ShowNewCategoryInput = true; });
            ShowUseCategoryCommand = new RelayCommand(() => { ShowNewCategoryInput = false; });
        }

        private async void SaveDocumentAsync()
        {
            EditingDocument.Category = ShowNewCategoryInput ? NewCategoryName : UseCategoryName;

            await documentService.SaveDocumentAsync(EditingDocument);

            // Delete removed photos
            if (originalDocument != null) {
                var deletedPhotos = originalDocument.Photos.Where(p => !EditingDocument.Photos.Contains(p)).Select(p => p.File);
                await documentService.RemovePhotosAsync(deletedPhotos);
            }

            documentService.DetachDocument(originalDocument);
            Category newCat = documentService.GetCategoryByName(EditingDocument.Category);
            newCat.Documents.Add(EditingDocument);

            var doc = EditingDocument;
            EditingDocument = null;
            originalDocument = doc;

            navigator.Navigate<IMainPage>(originalDocument.Id);
        }

        private async void AddPhotoFromCameraAsync()
        {
            var photo = await cameraService.CapturePhotoAsync();
            if (photo != null) {
                EditingDocument.Photos.Add(photo);
            }
        }

        private async void AddPhotoFromFileAsync()
        {
            IsBusy = true;
            var photos = await filePicker.PickMultiplePhotosAsync();
            IsBusy = false;
            foreach (var photo in photos) {
                EditingDocument.Photos.Add(photo);
            }
        }

        private void RemovePhoto()
        {
            EditingDocument.Photos.Remove(SelectedPhoto);
        }

        #endregion
    }
}
