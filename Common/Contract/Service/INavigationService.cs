using System;

namespace MyDocs.Common.Contract.Service
{
	public interface INavigationService
	{
		void GoBack();
		void Navigate(Type sourcePageType);
		void Navigate(Type sourcePageType, object parameter);
	}
}
