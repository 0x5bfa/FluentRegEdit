using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegistryValley.App.ViewModels.Dialogs;

namespace RegistryValley.App.Dialogs
{
    public sealed partial class KeyPermissionsViewerDialog : ContentDialog
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(KeyPermissionsViewerDialogViewModel),
                typeof(ValueViewerDialog),
                new PropertyMetadata(null)
                );

        public KeyPermissionsViewerDialogViewModel ViewModel
        {
            get => (KeyPermissionsViewerDialogViewModel)GetValue(ViewModelProperty);
            set
            {
                SetValue(ViewModelProperty, value);

                var command = value.LoadKeySecurityDescriptorCommand;
                if (command.CanExecute(null))
                    command.Execute(null);
            }
        }

        public KeyPermissionsViewerDialog()
        {
            InitializeComponent();
        }
    }
}
