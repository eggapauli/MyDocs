using GalaSoft.MvvmLight.Ioc;
using MyDocs.Common.Contract.Service;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyDocs.WindowsStore.Service
{
	public class NavigationService : INavigationService
	{
		public void Navigate<T>()
		{
			// TODO get type without creating an instance
			var type = SimpleIoc.Default.GetInstance<T>().GetType();
			((Frame)Window.Current.Content).Navigate(type);
		}

		public void Navigate<T>(object parameter)
		{
			// TODO get type without creating an instance
			var type = SimpleIoc.Default.GetInstance<T>().GetType();
			((Frame)Window.Current.Content).Navigate(type, parameter);
		}

		public void GoBack()
		{
			((Frame)Window.Current.Content).GoBack();
		}
	}
}
