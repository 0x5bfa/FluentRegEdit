using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using WinUIEx;

namespace RegistryValley.App
{
    public sealed partial class MainWindow : WindowEx
    {
        public MainWindow()
        {
            InitializeComponent();

            PersistenceId = "RegistryValleyMainWindow";

            EnsureEarlyWindow();
        }

        private void EnsureEarlyWindow()
        {
            // Set title
            AppWindow.Title = "RegistryValley";

            // Set icon
            AppWindow.SetIcon(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets/AppTiles/StoreLogo.png"));

            // Extend title bar
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

            // Set window buttons background to transparent
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // Set min size
            base.MinHeight = 328;
            base.MinWidth = 516;
        }

        public async Task InitializeApplication(object activatedEventArgs)
        {
        }

        private Frame EnsureWindowIsInitialized()
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(App.Window.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.CacheSize = 1;
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                App.Window.Content = rootFrame;
            }

            return rootFrame;
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
