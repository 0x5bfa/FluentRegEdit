using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RegistryValley.App.Models;
using Windows.Foundation.Metadata;
using Windows.UI.WindowManagement;

namespace RegistryValley.App.Views.Properties
{
    public sealed partial class SecurityAdvancedPage : Page
    {
        public SecurityAdvancedPage()
        {
            InitializeComponent();
        }

        public Microsoft.UI.Windowing.AppWindow AppWindow;

        public KeyItem KeyItem;
    }
}
