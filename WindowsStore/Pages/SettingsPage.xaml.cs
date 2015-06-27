using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace MyDocs.WindowsStore.Pages
{
    public sealed partial class SettingsPage : SettingsFlyout
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public void GoBack(object sender, RoutedEventArgs e)
        {
            Popup parent = Parent as Popup;
            if (parent != null) {
                parent.IsOpen = false;
            }

            // If the app is in fullscreen, then the back button shows the Settings pane again.
            if (Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsFullScreen) {
                SettingsPane.Show();
            }
        }
    }
}
