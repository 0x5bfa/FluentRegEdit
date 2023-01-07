using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegistryValley.App.Dialogs;
using RegistryValley.App.Extensions;
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ValuesViewerPage));
        }

        #region TreeView event methods
        private void CustomMainTreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnsureCurrentPageIsValuesViewer();

            // Load selected key's values
            if (CustomMainTreeView.SelectedItem != null
                && CustomMainTreeView.SelectedItem is KeyItem item
                && ValuesViewerViewModel.SelectedKeyItem != item)
            {
                ValuesViewerViewModel.SelectedKeyItem = item;
            }
        }

        private void ExpandCollapseButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((Button)sender).DataContext;

            if (!item.IsExpanded)
            {
                ExpandChildren(item);
            }
            else
            {
                CollapseChildren(item);
            }
        }
        #endregion

        #region MenuFlyout event methods
        private void KeyTreeViewItemMenuFlyout_Opening(object sender, object e)
        {
            var flyout = (MenuFlyout)sender;
            var target = (Grid)flyout.Target;
            var item = (KeyItem)target.DataContext;

            if (item.SelectedRootComputer && flyout.Items.Count > 5)
            {
                flyout.Items.RemoveAt(1);
                flyout.Items.RemoveAt(1);
                flyout.Items.RemoveAt(1);
                flyout.Items.RemoveAt(1);
                flyout.Items.RemoveAt(1);
                flyout.Items.RemoveAt(3);
                flyout.Items.RemoveAt(3);
                flyout.Items.RemoveAt(3);
                return;
            }
        }

        private void KeyTreeViewItemMenuFlyout_Opened(object sender, object e)
        {
            EnsureCurrentPageIsValuesViewer();

            var flyout = (MenuFlyout)sender;
            var target = (Grid)flyout.Target;
            var item = (KeyItem)target.DataContext;

            SelectItem(item);

            if (ValuesViewerViewModel.SelectedKeyItem != item)
                ValuesViewerViewModel.SelectedKeyItem = item;
        }

        private void KeyTreeViewItemMenuFlyoutExpand_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            ExpandChildren(item);
        }

        private void KeyTreeViewItemMenuFlyoutCollapse_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            CollapseChildren(item);
        }

        private async void KeyTreeViewItemMenuFlyoutNew_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)((MenuFlyoutItem)sender).DataContext;

            var dialog = new KeyAddingDialog()
            {
                KeyItem = item,
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

        private void KeyTreeViewItemMenuFlyoutRename_Click(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)CustomMainTreeView.SelectedItem;

            item.IsRenaming = true;
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

        #region Settings
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            CustomMainTreeView.SelectedIndex = -1;

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
        #endregion

        private void KeyItemNameRenamingTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Focus(FocusState.Programmatic);
        }

        private void KeyItemNameRenamingTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var item = (KeyItem)CustomMainTreeView.SelectedItem;
            item.IsRenaming = false;
        }

        private void KeyItemNameRenamingTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // TODO: Rename key

            }
        }

        private void GetChildItems(KeyItem item)
        {
            if (item.RootHive == HKEY.NULL)
                return;

            item.Children.Clear();

            var children = ViewModel.EnumerateRegistryKeys(item.RootHive, item.Path, item);
            if (children != null)
            {
                foreach (var child in children)
                    item.Children.Add(child);
            }
            else
            {
                // Error handling
                var result = Kernel32.GetLastError();
                if (result.Failed)
                {
                    ValuesViewerViewModel.StatusBarMessage = result.FormatMessage();
                }
            }
        }

        private void RemoveChildItems(KeyItem item)
        {
            item.Children.Clear();
            item.HasChildren = true;
        }

        private void ExpandChildren(KeyItem item)
        {
            item.IsExpanded = true;
            int index = ((ReadOnlyObservableCollection<KeyItem>)CustomMainTreeView.ItemsSource).IndexOf(item);

            var keyItemNodeTree = (ReadOnlyObservableCollection<KeyItem>)CustomMainTreeView.NodeItemSource;
            var flattenedKeyItemNodeTree = GetFlattenNodes(keyItemNodeTree);

            var itemFromFlattenedTree = flattenedKeyItemNodeTree.Where(x => x.PathForPwsh == item.PathForPwsh).FirstOrDefault();

            itemFromFlattenedTree.IsExpanded = true;

            if (item.Depth != 1)
                GetChildItems(itemFromFlattenedTree);

            index++;
            foreach (var child in itemFromFlattenedTree.Children)
            {
                ViewModel.Insert(index, child);
                index++;
            }
        }

        private void CollapseChildren(KeyItem item)
        {
            item.IsExpanded = false;
            int index = ((ReadOnlyObservableCollection<KeyItem>)CustomMainTreeView.ItemsSource).IndexOf(item);

            var keyItemNodeTree = (ReadOnlyObservableCollection<KeyItem>)CustomMainTreeView.NodeItemSource;
            var flattenedKeyItemNodeTree = GetFlattenNodes(keyItemNodeTree);

            var itemFromFlattenedTree = flattenedKeyItemNodeTree.Where(x => x.PathForPwsh == item.PathForPwsh).FirstOrDefault();

            itemFromFlattenedTree.IsExpanded = false;

            if (item.Depth != 1)
                RemoveChildItems(itemFromFlattenedTree);

            ViewModel.RemoveAll(item.Depth, index);
        }

        private void SelectItem(KeyItem item)
        {
            int index = ((ReadOnlyObservableCollection<KeyItem>)CustomMainTreeView.ItemsSource).IndexOf(item);

            CustomMainTreeView.SelectedIndex = index;
        }

        public IEnumerable<KeyItem> GetFlattenNodes(IEnumerable<KeyItem> masterList)
        {
            foreach (var node in masterList)
            {
                yield return node;

                foreach (var children in GetFlattenNodes(node.Children))
                {
                    yield return children;
                }
            }
        }
    }
}
