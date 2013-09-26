using System;

namespace MyDocs.Common.Contract.Service
{
    public interface INavigationService
    {
        bool CanGoBack { get; }
        void GoBack();
        void Navigate<T>();
        void Navigate<T>(object parameter);
    }
}
