namespace RegistryValley.App.Services
{
    internal sealed class UserSettingsServices : BaseSettingsServices
    {
        public bool RunAsAdminOnStartup
        {
            get => Get(true);
            set => Set(value);
        }
    }
}
