using MyDocs.Common.Contract.Service;
using System;

namespace Common.Test.Mocks
{
    class NavigationServiceMock : INavigationService
    {
        public bool CanGoBack
        {
            get { return true; }
        }

        public void GoBack()
        {
        }

        public Action<Type, object> NavigateAction = delegate { };
        public void Navigate<T>()
        {
            NavigateAction(typeof(T), null);
        }

        public void Navigate<T>(object parameter)
        {
            NavigateAction(typeof(T), parameter);
        }
    }
}
