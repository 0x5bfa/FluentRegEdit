using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using RegistryValley.App.Services;

namespace RegistryValley.App.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel()
        {
            _runAsAdminOnStartup = UserSettingsServices.RunAsAdminOnStartup;

            ColorModes = new List<string>()
            {
                "Default",
                "Light",
                "Dark",
            }
            .AsReadOnly();
        }

        private UserSettingsServices UserSettingsServices { get; } = App.Current.Services.GetRequiredService<UserSettingsServices>();

        public ReadOnlyCollection<string> ColorModes { get; set; }

        private int _selectedColorModeIndex = (int)Enum.Parse(typeof(ElementTheme), ThemeModeServices.RootTheme.ToString());
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

        private bool _runAsAdminOnStartup;
        public bool RunAsAdminOnStartup
        {
            get => _runAsAdminOnStartup;
            set
            {
                if (SetProperty(ref _runAsAdminOnStartup, value))
                {
                    UserSettingsServices.RunAsAdminOnStartup = value;
                }
            }
        }
    }
}
