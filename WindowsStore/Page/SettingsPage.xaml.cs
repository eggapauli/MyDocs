using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MyDocs.WindowsStore.Page
{
	public sealed partial class SettingsPage : UserControl
	{
		public SettingsPage()
		{
			this.InitializeComponent();
		}

		public void GoBack(object sender, RoutedEventArgs e)
		{
			Popup parent = this.Parent as Popup;
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
