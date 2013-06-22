using MyDocs.WindowsStoreFrontend.Common;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace MyDocs.WindowsStoreFrontend
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton Application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used when the application is launched to open a specific file, to display
		/// search results, and so forth.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs args)
		{
			SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;

			Frame rootFrame = Window.Current.Content as Frame;

			SuspensionManager.KnownTypes.Add(typeof(DateTimeOffset));
			SuspensionManager.KnownTypes.Add(typeof(string[]));
			SuspensionManager.KnownTypes.Add(typeof(bool[]));

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active

			if (rootFrame == null) {
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();
				//Associate the frame with a SuspensionManager key                                
				SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

				if (args.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					// Restore the saved session state only when appropriate
					try {
						await SuspensionManager.RestoreAsync();
					}
					catch (SuspensionManagerException) {
						//Something went wrong restoring state.
						//Assume there is no state and continue
					}
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}
			if (rootFrame.Content == null) {
				// When the navigation stack isn't restored navigate to the first page,
				// configuring the new page by passing required information as a navigation
				// parameter
				if (!rootFrame.Navigate(typeof(MainPage))) {
					throw new Exception("Failed to create initial page");
				}
			}
			// Ensure the current window is active
			Window.Current.Activate();
		}

		private ResourceLoader rl = new ResourceLoader();
		private void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
		{
			//SettingsCommand settingsCmd = new SettingsCommand("settings", rl.GetString("settings"), handler =>
			//{
			//	Popup popup = BuildSettingsItem(new SettingsPage(), 646);
			//	popup.IsOpen = true;
			//});
			//args.Request.ApplicationCommands.Add(settingsCmd);

			SettingsCommand privacyCmd = new SettingsCommand("privacy", rl.GetString("privacyStatement"), handler =>
			{
				var tmp = Launcher.LaunchUriAsync(new Uri("http://eggapauli.square7.ch/windows-store/my-docs/privacy.html"));
			});
			args.Request.ApplicationCommands.Add(privacyCmd);
		}

		private Popup BuildSettingsItem(UserControl control, int width)
		{
			Popup popup = new Popup();
			popup.IsLightDismissEnabled = true;
			popup.ChildTransitions = new TransitionCollection();
			popup.ChildTransitions.Add(new PaneThemeTransition() {
				Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
						EdgeTransitionLocation.Right :
						EdgeTransitionLocation.Left
			});

			control.Width = width;
			control.Height = Window.Current.Bounds.Height;
			popup.Child = control;

			popup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (Window.Current.Bounds.Width - width) : 0);
			popup.SetValue(Canvas.TopProperty, 0);

			return popup;
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private async void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			await SuspensionManager.SaveAsync();
			deferral.Complete();
		}

		/// <summary>
		/// Invoked when the application is activated to display search results.
		/// </summary>
		/// <param name="args">Details about the activation request.</param>
		protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
		{
			// TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
			// event in OnWindowCreated to speed up searches once the application is already running

			// If the Window isn't already using Frame navigation, insert our own Frame
			var previousContent = Window.Current.Content;
			var frame = previousContent as Frame;

			// If the app does not contain a top-level frame, it is possible that this 
			// is the initial launch of the app. Typically this method and OnLaunched 
			// in App.xaml.cs can call a common method.
			if (frame == null) {
				// Create a Frame to act as the navigation context and associate it with
				// a SuspensionManager key
				frame = new Frame();
				SuspensionManager.RegisterFrame(frame, "AppFrame");

				if (args.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					// Restore the saved session state only when appropriate
					try {
						await SuspensionManager.RestoreAsync();
					}
					catch (SuspensionManagerException) {
						//Something went wrong restoring state.
						//Assume there is no state and continue
					}
				}
			}

			frame.Navigate(typeof(SearchResultsPage), args.QueryText);
			Window.Current.Content = frame;

			// Ensure the current window is active
			Window.Current.Activate();
		}
	}
}
