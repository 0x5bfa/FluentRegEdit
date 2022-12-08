using RegistryValley.App.Models;

namespace RegistryValley.App.ViewModels.Dialogs
{
    public class KeyPermissionsViewerDialogViewModel : ObservableObject
    {
        public KeyPermissionsViewerDialogViewModel()
        {

        }

        private KeyItem _keyItem;
        public KeyItem KeyItem { get => _keyItem; set => SetProperty(ref _keyItem, value); }

    }
}
