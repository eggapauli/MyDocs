/*
  In App.xaml:
  <Application.Resources>
	  <vm:ViewModelLocator xmlns:vm="clr-namespace:MyDocs"
						   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using MyDocs.Common;
using MyDocs.Contract;
using MyDocs.Contract.Service;
using MyDocs.Service;
using MyDocs.Service.Design;
using MyDocs.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace MyDocs.ViewModel
{
	/// <summary>
	/// This class contains static references to all the view models in the
	/// application and provides an entry point for the bindings.
	/// </summary>
	public class ViewModelLocator
	{
		/// <summary>
		/// Initializes a new instance of the ViewModelLocator class.
		/// </summary>
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

			SimpleIoc.Default.Register<DocumentViewModel>();
			SimpleIoc.Default.Register<EditDocumentViewModel>();
			SimpleIoc.Default.Register<SearchViewModel>();
			SimpleIoc.Default.Register<SettingsViewModel>();
		}

		public DocumentViewModel DocumentVM
		{
			get
			{
				return ServiceLocator.Current.GetInstance<DocumentViewModel>();
			}
		}

		public EditDocumentViewModel EditDocumentVM
		{
			get
			{
				return ServiceLocator.Current.GetInstance<EditDocumentViewModel>();
			}
		}

		public SearchViewModel SearchVM
		{
			get
			{
				return ServiceLocator.Current.GetInstance<SearchViewModel>();
			}
		}

		public SettingsViewModel SettingsVM
		{
			get
			{
				return ServiceLocator.Current.GetInstance<SettingsViewModel>();
			}
		}

		public static void Cleanup()
		{
			// TODO Clear the ViewModels
		}
	}
}