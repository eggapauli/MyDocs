using MyDocs.Common.Contract.Service;
using Splat;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyDocs.WindowsStore.Service
{
    public class NavigationService : INavigationService
    {
        private Frame Frame
        {
            get { return (Frame)Window.Current.Content; }
        }

        public bool CanGoBack
        {
            get { return Frame.CanGoBack; }
        }

        public void Navigate<T>()
        {
            // TODO get type without creating an instance
            var type = Locator.Current.GetService<T>().GetType();
            Frame.Navigate(type);
        }

        public void Navigate<T>(object parameter)
        {
            // TODO get type without creating an instance
            var type = Locator.Current.GetService<T>().GetType();
            Frame.Navigate(type, parameter);
        }

        public void GoBack()
        {
            Frame.GoBack();
        }
    }
}
