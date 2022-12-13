using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegistryValley.App.Dialogs;
using RegistryValley.App.Models;
using RegistryValley.App.ViewModels;

namespace RegistryValley.App.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            var provider = App.Current.Services;
            ViewModel = provider.GetRequiredService<MainViewModel>();
            ValuesViewerViewModel = provider.GetRequiredService<ValuesViewerViewModel>();
        }

        #region Fields and Properties
        public MainViewModel ViewModel { get; }

        public ValuesViewerViewModel ValuesViewerViewModel { get; }
        #endregion

        #region Event methods
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ValuesViewerPage));
        }

        private void KeyTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            var item = (KeyItem)args.Item;
            if (item.RootHive != HKEY.NULL)
            {
                item.Children.Clear();

                var children = ViewModel.EnumerateRegistryKeys(item.RootHive, item.Path);
                if (children != null)
                {
                    foreach (var child in children)
                        item.Children.Add(child);
                }
            }
        }

        private void KeyTreeView_Collapsed(TreeView sender, TreeViewCollapsedEventArgs args)
        {
            var item = (KeyItem)args.Item;
            item.Children.Clear();
        }

        private void KeyTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            EnsureCurrentPageIsValuesViewer();

            var item = (KeyItem)args.InvokedItem;
            ValuesViewerViewModel.SelectedKeyItem = item;
        }

        private async void KeyTreeViewItemMenuFlyoutPermissions_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).Tag;

            var dialog = new KeyPermissionsViewerDialog
            {
                ViewModel = new()
                {
                    KeyItem = item,
                },

                // WinUI3: https://github.com/microsoft/microsoft-ui-xaml/issues/2504
                XamlRoot = this.Content.XamlRoot,
            };

            var result = await dialog.ShowAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValuesViewerViewModel.SelectedKeyItem != null)
                ValuesViewerViewModel.SelectedKeyItem.IsSelected = false;
            SettingsButtonClickedIndicator.Visibility = Visibility.Visible;
            SettingsButtonClickedBackground.Visibility = Visibility.Visible;

            ContentFrame.Navigate(typeof(SettingsPage));
        }
        #endregion

        private void EnsureCurrentPageIsValuesViewer()
        {
            var currentSourcePageType = ContentFrame.CurrentSourcePageType;

            if (currentSourcePageType == typeof(SettingsPage))
            {
                SettingsButtonClickedIndicator.Visibility = Visibility.Collapsed;
                SettingsButtonClickedBackground.Visibility = Visibility.Collapsed;
                ContentFrame.Navigate(typeof(ValuesViewerPage));
            }
        }
    }
}
