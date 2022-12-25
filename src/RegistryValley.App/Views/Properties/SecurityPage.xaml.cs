using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using RegistryValley.App.Models;
using RegistryValley.App.ViewModels.Properties;

namespace RegistryValley.App.Views.Properties
{
    public sealed partial class SecurityPage : Page
    {
        public SecurityPage()
        {
            InitializeComponent();

            var provider = App.Current.Services;
            ViewModel = provider.GetRequiredService<SecurityViewModel>();
        }

        public SecurityViewModel ViewModel { get; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.KeyItem = e.Parameter as KeyItem;
            ViewModel.GetKeyAccessControlList();
        }

        private void ViewAdvancedSecurityButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void MergedPermissionPrincipalsListView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var listView = (ListView)sender;

            if (listView.ItemsSource != null && ViewModel.Principals.Count != 0)
            {
                // Select first item
                listView.SelectedIndex = 0;
            }
        }
    }
}
