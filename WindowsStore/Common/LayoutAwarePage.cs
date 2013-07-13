﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Controls = Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyDocs.WindowsStore.Common
{
    /// <summary> 
    /// Typical implementation of Page that provides several important conveniences: 
    /// <list type="bullet"> 
    /// <item> 
    /// <description>Application view state to visual state mapping</description> 
    /// </item> 
    /// <item> 
    /// <description>GoBack, GoForward, and GoHome event handlers</description> 
    /// </item> 
    /// <item> 
    /// <description>Mouse and keyboard shortcuts for navigation</description> 
    /// </item> 
    /// <item> 
    /// <description>State management for navigation and process lifetime management</description> 
    /// </item> 
    /// <item> 
    /// <description>A default view model</description> 
    /// </item> 
    /// </list> 
    /// </summary> 
    [Windows.Foundation.Metadata.WebHostHidden]
    public class LayoutAwarePage : Controls.Page
    {
        private NavigationHelper navigationHelper;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private List<Controls.Control> _layoutAwareControls;

        /// <summary> 
        /// Initializes a new instance of the <see cref="LayoutAwarePage"/> class. 
        /// </summary> 
        public LayoutAwarePage()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += LoadState;
            this.navigationHelper.SaveState += SaveState;

            // When this page is part of the visual tree make the following change: 
            // * Map application view state to visual state for the page 
            this.Loaded += (sender, e) =>
            {
                this.StartLayoutUpdates(sender, e);
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        protected virtual void LoadState(object sender, LoadStateEventArgs e)
        {
        }

        protected virtual void SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary> 
        /// Invoked as an event handler, typically on the <see cref="FrameworkElement.Loaded"/> 
        /// event of a <see cref="Control"/> within the page, to indicate that the sender should 
        /// start receiving visual state management changes that correspond to application view 
        /// state changes. 
        /// </summary> 
        /// <param name="sender">Instance of <see cref="Control"/> that supports visual state 
        /// management corresponding to view states.</param> 
        /// <param name="e">Event data that describes how the request was made.</param> 
        /// <remarks>The current view state will immediately be used to set the corresponding 
        /// visual state when layout updates are requested.  A corresponding 
        /// <see cref="FrameworkElement.Unloaded"/> event handler connected to 
        /// <see cref="StopLayoutUpdates"/> is strongly encouraged.  Instances of 
        /// <see cref="LayoutAwarePage"/> automatically invoke these handlers in their Loaded and 
        /// Unloaded events.</remarks> 
        /// <seealso cref="DetermineVisualState"/> 
        /// <seealso cref="InvalidateVisualState"/> 
        public void StartLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Controls.Control;
            if (control == null) return;
            if (this._layoutAwareControls == null)
            {
                // Start listening to view state changes when there are controls interested in updates 
                Window.Current.SizeChanged += this.WindowSizeChanged;
                this._layoutAwareControls = new List<Controls.Control>();
            }
            this._layoutAwareControls.Add(control);

            // Set the initial visual state of the control 
            VisualStateManager.GoToState(control, DetermineVisualState(Window.Current.Bounds.Width), false);
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            this.InvalidateVisualState();
        }

        /// <summary> 
        /// Invoked as an event handler, typically on the <see cref="FrameworkElement.Unloaded"/> 
        /// event of a <see cref="Control"/>, to indicate that the sender should start receiving 
        /// visual state management changes that correspond to application view state changes. 
        /// </summary> 
        /// <param name="sender">Instance of <see cref="Control"/> that supports visual state 
        /// management corresponding to view states.</param> 
        /// <param name="e">Event data that describes how the request was made.</param> 
        /// <remarks>The current view state will immediately be used to set the corresponding 
        /// visual state when layout updates are requested.</remarks> 
        /// <seealso cref="StartLayoutUpdates"/> 
        public void StopLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Controls.Control;
            if (control == null || this._layoutAwareControls == null) return;
            this._layoutAwareControls.Remove(control);
            if (this._layoutAwareControls.Count == 0)
            {
                // Stop listening to view state changes when no controls are interested in updates 
                this._layoutAwareControls = null;
                Window.Current.SizeChanged -= this.WindowSizeChanged;
            }
        }

        /// <summary> 
        /// Translates <see cref="double"/> values into strings for visual state 
        /// management within the page.  The default implementation uses the names of enum values. 
        /// Subclasses may override this method to control the mapping scheme used. 
        /// </summary> 
        /// <param name="viewState">View state for which a visual state is desired.</param> 
        /// <returns>Visual state name used to drive the 
        /// <see cref="VisualStateManager"/></returns> 
        /// <seealso cref="InvalidateVisualState"/> 
        protected virtual string DetermineVisualState(double width)
        {
            return (width < 768) ? "TightLayout" : "DefaultLayout";
        }

        /// <summary> 
        /// Updates all controls that are listening for visual state changes with the correct 
        /// visual state. 
        /// </summary> 
        /// <remarks> 
        /// Typically used in conjunction with overriding <see cref="DetermineVisualState"/> to 
        /// signal that a different value may be returned even though the view state has not 
        /// changed. 
        /// </remarks> 
        public void InvalidateVisualState()
        {
            if (this._layoutAwareControls != null)
            {
                string visualState = DetermineVisualState(Window.Current.Bounds.Width);
                foreach (var layoutAwareControl in this._layoutAwareControls)
                {
                    VisualStateManager.GoToState(layoutAwareControl, visualState, false);
                }
            }
        }
    }
}