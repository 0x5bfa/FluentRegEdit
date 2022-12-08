using RegistryValley.App.Models;

namespace RegistryValley.App.ViewModels.Dialogs
{
    public class ValueViewerDialogViewModel : ObservableObject
    {
        public ValueViewerDialogViewModel()
        {
        }

        private ValueItem _valueItem;
        public ValueItem ValueItem { get => _valueItem; set => SetProperty(ref _valueItem, value); }
    }
}
