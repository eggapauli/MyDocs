using System;

namespace MyDocs.Contract.Service
{
	public interface INavigationService
	{
		void GoBack();
		void Navigate(Type sourcePageType);
		void Navigate(Type sourcePageType, object parameter);
	}
}
