using MyDocs.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyDocs.Service
{
	public class NavigationService : INavigationService
	{
		public void Navigate(Type sourcePageType)
		{
			((Frame)Window.Current.Content).Navigate(sourcePageType);
		}
		public void Navigate(Type sourcePageType, object parameter)
		{
			((Frame)Window.Current.Content).Navigate(sourcePageType, parameter);
		}
		public void GoBack()
		{
			((Frame)Window.Current.Content).GoBack();
		}
	}
}
