namespace RegistryValley.App.Services
{
    internal sealed class UserSettingsServices : BaseSettingsServices
    {
        public bool RunAsAdminOnStartup
        {
            get => Get(true);
            set => Set(value);
        }

        public string SelectedAppTheme
        {
            get => Get("Default");
            set => Set(value);
        }

        public bool SetupCompleted
        {
            get => Get(false);
            set => Set(value);
        }
    }
}
