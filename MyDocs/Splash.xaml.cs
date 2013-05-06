using MyDocs.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyDocs
{
	public sealed partial class Splash : Page
	{
		private SplashScreen splashScreen;
		private Rect splashImage;
		private Frame frame;

		public Splash(SplashScreen splashScreen, Frame frame)
		{
			this.InitializeComponent();
			this.frame = frame;

			this.splashScreen = splashScreen;
			splashImage = splashScreen.ImageLocation;
			splashScreen.Dismissed += new TypedEventHandler<SplashScreen, Object>(splashScreen_Dismissed);
			PositionSplashScreen();
		}

		public DocumentViewModel ViewModel
		{
			get { return this.DataContext as DocumentViewModel; }
		}

		private async void splashScreen_Dismissed(SplashScreen sender, object args)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				ViewModel.LoadAsync().ContinueWith(t =>
				{
					if (t.IsFaulted) {
						var tmp2 = ShowErrorAsync("loadCategoriesError");
					}
					else {
						Window.Current.Content = frame;
						frame.Navigate(typeof(MainPage));
						Window.Current.Activate();
					}
				}, TaskScheduler.FromCurrentSynchronizationContext());
			});
		}

		private ResourceLoader rl = new ResourceLoader();
		private async Task<IUICommand> ShowErrorAsync(string msgKey)
		{
			string msg = rl.GetString(msgKey);
			if (String.IsNullOrEmpty(msg)) {
				msg = "An error occured.";
			}
			return await new MessageDialog(msg).ShowAsync().AsTask();
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			splashScreen.Dismissed -= splashScreen_Dismissed;
		}

		private void PositionSplashScreen()
		{
			SplashScreenImage.SetValue(Canvas.TopProperty, splashImage.Y);
			SplashScreenImage.SetValue(Canvas.LeftProperty, splashImage.X);
			SplashScreenImage.Height = splashImage.Height;
			SplashScreenImage.Width = splashImage.Width;
			SplashScreenImage.Visibility = Visibility.Visible;
		}
	}
}
