using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegistryValley.App.Dialogs;
using RegistryValley.App.Models;
using RegistryValley.App.ViewModels;
using RegistryValley.Core.Services;
using WinRT.Interop;

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

        #region TreeView event methods
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ValuesViewerPage));
        }

        private void KeyTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            var item = (KeyItem)args.Item;
            ExpandChildItems(item);
        }

        private void KeyTreeView_Collapsed(TreeView sender, TreeViewCollapsedEventArgs args)
        {
            var item = (KeyItem)args.Item;
            CollapseChildItems(item);
        }

        private void KeyTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            EnsureCurrentPageIsValuesViewer();

            var item = (KeyItem)args.InvokedItem;

            if (ValuesViewerViewModel.SelectedKeyItem != item)
                ValuesViewerViewModel.SelectedKeyItem = item;
        }
        #endregion

        #region MenuFlyout event methods
        private void KeyTreeViewItemMenuFlyout_Opened(object sender, object e)
        {
            EnsureCurrentPageIsValuesViewer();

            var flyout = (MenuFlyout)sender;
            var target = (TreeViewItem)flyout.Target;
            var item = (KeyItem)target.DataContext;
            item.IsSelected = true;

            if (ValuesViewerViewModel.SelectedKeyItem != item)
                ValuesViewerViewModel.SelectedKeyItem = item;
        }

        private void KeyTreeViewItemMenuFlyoutExpand_Click(object sender, RoutedEventArgs e)
        {
            // Triggering an event KeyTreeView_Expanding
            ((KeyItem)((MenuFlyoutItem)sender).DataContext).IsExpanded = true;
        }

        private void KeyTreeViewItemMenuFlyoutCollapse_Click(object sender, RoutedEventArgs e)
        {
            // Triggering an event KeyTreeView_Collapsed
            ((KeyItem)((MenuFlyoutItem)sender).DataContext).IsExpanded = false;
        }

        private async void KeyTreeViewItemMenuFlyoutNew_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            var dialog = new KeyAddingDialog
            {
                KeyItem = item,
                // WinUI3: https://github.com/microsoft/microsoft-ui-xaml/issues/2504
                XamlRoot = Content.XamlRoot,
            };

            var result = await dialog.ShowAsync();
        }

        private async void KeyTreeViewItemMenuFlyoutDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = ((KeyItem)((MenuFlyoutItem)sender).DataContext);

            ContentDialog dialog = new()
            {
                Title = "Confirm key deletion",
                Content = "Are you sure you want permanently\ndelete this key and all of its subkeys?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                XamlRoot = Content.XamlRoot,
            };

            // Make sure user want to
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary)
                return;

            var command = ViewModel.DeleteKeyCommand;
            if (command.CanExecute(item))
                command.Execute(item);

            // TODO: Get command running status before getting result
            //var result = command.ExecutionTask.GetResultOrDefault();
        }

        private async void KeyTreeViewItemMenuFlyoutRename_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            var dialog = new KeyRenamingDialog
            {
                KeyItem = item,
                // WinUI3: https://github.com/microsoft/microsoft-ui-xaml/issues/2504
                XamlRoot = Content.XamlRoot,
            };

            var result = await dialog.ShowAsync();
        }

        private async void KeyTreeViewItemMenuFlyoutExport_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.FileTypeChoices.Add("Registration Entries", new List<string>() { ".reg" });

            InitializeWithWindow.Initialize(picker, App.WindowHandle);

            var file = await picker.PickSaveFileAsync();

            if (file != null)
                ShellServices.RunCmdPromptCommand(runAs: true, $"/c REG EXPORT {item.PathForCmd} {file.Path}");
        }

        private void KeyTreeViewItemMenuFlyoutPermissions_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            Helpers.PropertyWindowHelpers.CreatePropertyWindow(item);
        }

        private void KeyTreeViewItemMenuFlyoutCopyKeyName_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dp.SetText(item.PathForPwsh);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }
        #endregion

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValuesViewerViewModel.SelectedKeyItem != null)
                ValuesViewerViewModel.SelectedKeyItem.IsSelected = false;

            SettingsButtonClickedIndicator.Visibility = Visibility.Visible;
            SettingsButtonClickedBackground.Visibility = Visibility.Visible;

            ContentFrame.Navigate(typeof(SettingsPage));
        }

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

        private void ExpandChildItems(KeyItem item)
        {
            if (item.RootHive != HKEY.NULL)
            {
                item.Children.Clear();

                var children = ViewModel.EnumerateRegistryKeys(item.RootHive, item.Path, item);
                if (children != null)
                {
                    foreach (var child in children)
                        item.Children.Add(child);
                }
                else
                {
                    var result = Kernel32.GetLastError();
                    if (result.Failed)
                    {
                        ValuesViewerViewModel.StatusBarMessage = result.FormatMessage();
                    }
                }
            }
        }

        private void CollapseChildItems(KeyItem item)
        {
            item.Children.Clear();
            item.HasUnrealizedChildren = true;
        }
    }
}
