using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using MyDocs.Common.Contract.Page;
using MyDocs.Common.Contract.Service;
using MyDocs.Common.ViewModel;
using MyDocs.WindowsStore.Pages;
using MyDocs.WindowsStore.Service;
using MyDocs.WindowsStore.Service.Design;
using MyDocs.WindowsStore.Storage;
using System;

namespace MyDocs.WindowsStore.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic) {
                Register<IDocumentService, DesignDocumentService>();
            }
            else {
                Register<IDocumentService, DocumentService>();
            }

            Register<IPersistDocuments, ApplicationDataContainerDocumentStorage>();
            Register<INavigationService, NavigationService>();
            Register<ISettingsService, SettingsService>();
            Register<IUserInterfaceService, ModernUIService>();
            Register<ICameraService, CameraService>();
            Register<IFileOpenPickerService, FileOpenPickerService>();
            Register<IFileSavePickerService, FileSavePickerService>();
            Register<IPageExtractor>(GetPageExtractor);
            Register<ITranslatorService, TranslatorService>();
            Register<ILicenseService, LicenseService>();
            Register<IMainPage, MainPage>();
            Register<IEditDocumentPage, EditDocumentPage>();
            Register<IShowDocumentPage, ShowDocumentPage>();
            Register<ISearchPage, SearchResultsPage>();

            SimpleIoc.Default.Register<DocumentViewModel>();
            SimpleIoc.Default.Register<EditDocumentViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        private void Register<TInterface, TClass>()
            where TInterface : class
            where TClass : class
        {
            if (!SimpleIoc.Default.IsRegistered<TInterface>()) {
                SimpleIoc.Default.Register<TInterface, TClass>();
            }
        }

        private void Register<TInterface>(Func<TInterface> factory)
            where TInterface : class
        {
            if (!SimpleIoc.Default.IsRegistered<TInterface>()) {
                SimpleIoc.Default.Register<TInterface>(factory);
            }
        }

        private IPageExtractor GetPageExtractor()
        {
            return new PageExtractorList(
                new IPageExtractor[] { new PdfPageExtractor() });
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