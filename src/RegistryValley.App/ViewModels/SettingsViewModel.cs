using Microsoft.UI.Xaml;
using RegistryValley.App.Services;

namespace RegistryValley.App.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel()
        {
            ColorModes = new List<string>()
            {
                "Default",
                "Light",
                "Dark",
            }
            .AsReadOnly();
        }

        public ReadOnlyCollection<string> ColorModes { get; set; }

        private int _selectedColorModeIndex;
        public int SelectedColorModeIndex
        {
            get => _selectedColorModeIndex;
            set
            {
                if (SetProperty(ref _selectedColorModeIndex, value))
                {
                    ThemeModeServices.RootTheme = (ElementTheme)value;
                }
            }
        }
    }
}
