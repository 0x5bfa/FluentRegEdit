using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
        }

        public MainViewModel ViewModel { get; }

        private void OnDirTreeViewExpanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            ViewModel.Loading = true;

            var item = (KeyItem)args.Item;

            if (args.Node.HasUnrealizedChildren && item.RootHive != HKEY.NULL)
            {
                item.Children.Clear();

                var children = ViewModel.RegValleyEnumKeys(item.RootHive, item.Path);

                if (children != null)
                {
                    foreach (var child in children)
                        item.Children.Add(child);

                    args.Node.HasUnrealizedChildren = false;
                }
            }

            ViewModel.Loading = false;
        }

        private void OnDirTreeViewItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            ViewModel.Loading = true;
            var item = (KeyItem)args.InvokedItem;

            if (item.RootHive != HKEY.NULL)
            {
                ViewModel.RegValleyEnumValues(item.RootHive, item.Path, item.Name);
            }

            ViewModel.Loading = false;
        }

        private void OnValueListViewDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            //var item = ValueListView.SelectedItem as Models.RegistryValueModel;
            //var dialog = new Dialogs.ValueViewerDialog();
            //dialog.ViewModel = new()
            //{
            //    ValueModel = item,
            //};
            //await dialog.ShowAsync();
        }
    }
}
