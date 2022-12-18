using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegistryValley.App.Models;

namespace RegistryValley.App.Dialogs
{
    public sealed partial class KeyRenamingDialog : ContentDialog
    {
        public static readonly DependencyProperty KeyItemProperty =
            DependencyProperty.Register(
                nameof(KeyItem),
                typeof(KeyItem),
                typeof(KeyAddingDialog),
                new PropertyMetadata(null));

        public KeyItem KeyItem
        {
            get => (KeyItem)GetValue(KeyItemProperty);
            set
            {
                SetValue(KeyItemProperty, value);
            }
        }

        public KeyRenamingDialog()
        {
            this.InitializeComponent();
        }
    }
}
