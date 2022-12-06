using Microsoft.Win32;
using RegistryValley.App.Models;

namespace RegistryValley.App.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            _hiveSource = new();
            HiveSource = new(_hiveSource);

            InitializeRegistryRoot();
        }

        public readonly ObservableCollection<HiveItem> _hiveSource;
        public ReadOnlyObservableCollection<HiveItem> HiveSource { get; }

        private readonly string folderImageSource = "ms-appx:///Assets/Images/Folder.png";
        private readonly string computerImageSource = "ms-appx:///Assets/Images/Computer.png";
        private readonly string binaryImageSource = "ms-appx:///Assets/Images/Binary.png";
        private readonly string stringImageSource = "ms-appx:///Assets/Images/String.png";

        private void InitializeRegistryRoot()
        {
            List<HiveItem> list = new()
            {
                new()
                {
                    Name = "HKEY_CLASSES_ROOT",
                    Image = folderImageSource,
                    Hive = RegistryHive.ClassesRoot,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_CURRENT_USER",
                    Image = folderImageSource,
                    Hive = RegistryHive.CurrentUser,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_LOCAL_MACHINE",
                    Image = folderImageSource,
                    Hive = RegistryHive.LocalMachine,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_USERS",
                    Image = folderImageSource,
                    Hive = RegistryHive.Users,
                    Path = ""
                },
                new()
                {
                    Name = "HKEY_CURRENT_CONFIG",
                    Image = folderImageSource,
                    Hive = RegistryHive.CurrentConfig,
                    Path = ""
                }
            };

            foreach (HiveItem element in list)
            {
                try
                {
                    string[] keys = Array.Empty<string>();

                    switch (element.Hive)
                    {
                        case RegistryHive.ClassesRoot:
                            keys = Registry.ClassesRoot.GetSubKeyNames();
                            break;
                        case RegistryHive.CurrentConfig:
                            keys = Registry.CurrentConfig.GetSubKeyNames();
                            break;
                        case RegistryHive.CurrentUser:
                            keys = Registry.CurrentUser.GetSubKeyNames();
                            break;
                        case RegistryHive.LocalMachine:
                            keys = Registry.LocalMachine.GetSubKeyNames();
                            break;
                        case RegistryHive.Users:
                            keys = Registry.Users.GetSubKeyNames();
                            break;
                    }

                    foreach (string key in keys)
                    {
                        element.Children.Add(new()
                        {
                            Name = key,
                            Hive = element.Hive,
                            Image = folderImageSource,
                            Path = element.Path + key + "\\"
                        });
                    }
                }
                catch
                {

                }
            }

            _hiveSource.Add(new()
            {
                Name = "Computer",
                Children = new(list),
                Image = computerImageSource,
                Expanded = true
            });
        }
    }
}
