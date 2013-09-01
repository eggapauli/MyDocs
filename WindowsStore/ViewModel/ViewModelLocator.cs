using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Pages;
using MyDocs.WindowsStore.Service;
using MyDocs.WindowsStore.Service.Design;

namespace MyDocs.WindowsStore.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (!SimpleIoc.Default.IsRegistered<IDocumentService>()) {
                if (ViewModelBase.IsInDesignModeStatic) {
                    SimpleIoc.Default.Register<IDocumentService, DesignDocumentService>();
                }
                else {
                    SimpleIoc.Default.Register<IDocumentService, DocumentService>();
                }
            }

            if (!SimpleIoc.Default.IsRegistered<INavigationService>()) {
                SimpleIoc.Default.Register<INavigationService, NavigationService>();
            }

            if (!SimpleIoc.Default.IsRegistered<ISettingsService>()) {
                SimpleIoc.Default.Register<ISettingsService, SettingsService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IUserInterfaceService>()) {
                SimpleIoc.Default.Register<IUserInterfaceService, ModernUIService>();
            }

            if (!SimpleIoc.Default.IsRegistered<ICameraService>()) {
                SimpleIoc.Default.Register<ICameraService, CameraService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IFileOpenPickerService>()) {
                SimpleIoc.Default.Register<IFileOpenPickerService, FileOpenPickerService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IFileSavePickerService>()) {
                SimpleIoc.Default.Register<IFileSavePickerService, FileSavePickerService>();
            }

            if (!SimpleIoc.Default.IsRegistered<ITranslatorService>()) {
                SimpleIoc.Default.Register<ITranslatorService, TranslatorService>();
            }

            if (!SimpleIoc.Default.IsRegistered<ILicenseService>()) {
                SimpleIoc.Default.Register<ILicenseService, LicenseService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IMainPage>()) {
                SimpleIoc.Default.Register<IMainPage, MainPage>();
            }

            if (!SimpleIoc.Default.IsRegistered<IEditDocumentPage>()) {
                SimpleIoc.Default.Register<IEditDocumentPage, EditDocumentPage>();
            }

            if (!SimpleIoc.Default.IsRegistered<IShowDocumentPage>()) {
                SimpleIoc.Default.Register<IShowDocumentPage, ShowDocumentPage>();
            }

            if (!SimpleIoc.Default.IsRegistered<ISearchPage>()) {
                SimpleIoc.Default.Register<ISearchPage, SearchResultsPage>();
            }

            SimpleIoc.Default.Register<DocumentViewModel>();
            SimpleIoc.Default.Register<EditDocumentViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        public DocumentViewModel DocumentVM
        {
            get { return ServiceLocator.Current.GetInstance<DocumentViewModel>(); }
        }

        public EditDocumentViewModel EditDocumentVM
        {
            get { return ServiceLocator.Current.GetInstance<EditDocumentViewModel>(); }
        }

        public SearchViewModel SearchVM
        {
            get { return ServiceLocator.Current.GetInstance<SearchViewModel>(); }
        }

        public SettingsViewModel SettingsVM
        {
            get { return ServiceLocator.Current.GetInstance<SettingsViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}