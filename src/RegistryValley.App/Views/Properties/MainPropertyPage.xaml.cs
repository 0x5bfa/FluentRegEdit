using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RegistryValley.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.WindowManagement;

namespace RegistryValley.App.Views.Properties
{
    public sealed partial class MainPropertyPage : Page
    {
        public MainPropertyPage()
        {
            InitializeComponent();
        }

        public Microsoft.UI.Windowing.AppWindow AppWindow;

        public KeyItem KeyItem;

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            switch (args.SelectedItemContainer.Tag)
            {
                case "General":
                    contentFrame.Navigate(typeof(GeneralPage), KeyItem, args.RecommendedNavigationTransitionInfo);
                    break;

                case "Security":
                    contentFrame.Navigate(typeof(SecurityPage), KeyItem, args.RecommendedNavigationTransitionInfo);
                    break;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Save and sync

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
                AppWindow.Destroy();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
                AppWindow.Destroy();
        }
    }
}
