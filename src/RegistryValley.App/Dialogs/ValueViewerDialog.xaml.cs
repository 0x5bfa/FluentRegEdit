using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegistryValley.App.ViewModels;
using RegistryValley.App.ViewModels.Dialogs;

namespace RegistryValley.App.Dialogs
{
    public sealed partial class ValueViewerDialog : ContentDialog
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ValueViewerDialogViewModel),
                typeof(ValueViewerDialog),
                new PropertyMetadata(null));

        public ValueViewerDialogViewModel ViewModel
        {
            get => (ValueViewerDialogViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ValueViewerDialog()
            => InitializeComponent();

        private void OnValueEditorTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}
