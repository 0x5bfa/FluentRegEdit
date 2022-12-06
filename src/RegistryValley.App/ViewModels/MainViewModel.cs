using Microsoft.Win32;
using RegistryValley.App.Models;
using RegistryValley.App.UserControls.TreeViewControl;

namespace RegistryValley.App.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            DataContainer = new();
        }

        public DataContainer DataContainer { get; set; }
    }
}
