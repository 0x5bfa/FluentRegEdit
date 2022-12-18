using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using RegistryValley.App.Models;
using RegistryValley.App.ViewModels.Dialogs;

namespace RegistryValley.App.Dialogs
{
    public sealed partial class KeyPermissionsViewerDialog : ContentDialog
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(KeyPermissionsViewerDialogViewModel),
                typeof(ValueEditingDialog),
                new PropertyMetadata(null));

        public KeyPermissionsViewerDialogViewModel ViewModel
        {
            get => (KeyPermissionsViewerDialogViewModel)GetValue(ViewModelProperty);
            set
            {
                SetValue(ViewModelProperty, value);

                ViewModel.ShowAdvancedPermissions = false;

                var command = ViewModel.LoadKeySecurityCommand;
                if (command.CanExecute(null))
                    command.Execute(null);
            }
        }

        public KeyPermissionsViewerDialog()
        {
            InitializeComponent();
        }

        private void OnViewAdvancedSecurityButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadKeySecurityOwner();
            ViewModel.ShowAdvancedPermissions = true;
        }

        private void OnBackToSummaryPermissionsGridButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadKeySecurityDescriptor();
            ViewModel.ShowAdvancedPermissions = false;
        }

        private void OnAdvancedPermissionListViewItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var item = (PermissionPrincipalItem)AdvancedPermissionListView.SelectedItem;

            ViewModel.SelectedPrincipalAdvancedPermission = item;

            ViewModel.SelectedAdvancedPermissionItem = true;
        }

        private void OnBackToAdvancedPermissionsGridButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowAdvancedPermissions = true;
            ViewModel.SelectedAdvancedPermissionItem = false;
        }
    }
}
