using System;

namespace MyDocs.Common.Contract.Service
{
    public interface INavigationService
    {
        void GoBack();
        void Navigate<T>();
        void Navigate<T>(object parameter);
    }
}
