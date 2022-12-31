using RegistryValley.Core.Services;
using RegistryValley.App.Models;
using static RegistryValley.Core.Helpers.RegistryServices;

namespace RegistryValley.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            _keyItems = new();
            KeyItems = new(_keyItems);

            DeleteKeyCommand = new RelayCommand<KeyItem>(DeleteSelectedKey);

            InitializeHiveTree();
        }

        #region Fields and Properties
        private readonly ObservableCollection<KeyItem> _keyItems;
        public ReadOnlyObservableCollection<KeyItem> KeyItems { get; }

        private bool _loading;
        public bool Loading { get => _loading; set => SetProperty(ref _loading, value); }

        public IRelayCommand DeleteKeyCommand { get; }
        #endregion

        private void InitializeHiveTree()
        {
            _keyItems.Add(new()
            {
                Name = "Computer",
                RootHive = HKEY.NULL,
                IsExpanded = true,
                IsSelected = true,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasUnrealizedChildren = true,
                SelectedRootComputer = true,
                Image = "ms-appx:///Assets/Images/Computer.png",
                Children = new()
                {
                    new()
                    {
                        Name = "HKEY_CLASSES_ROOT",
                        RootHive = HKEY.HKEY_CLASSES_ROOT,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasUnrealizedChildren = true,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_CURRENT_USER",
                        RootHive = HKEY.HKEY_CURRENT_USER,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasUnrealizedChildren = true,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_LOCAL_MACHINE",
                        RootHive = HKEY.HKEY_LOCAL_MACHINE,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasUnrealizedChildren = true,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_USERS",
                        RootHive = HKEY.HKEY_USERS,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasUnrealizedChildren = true,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_CURRENT_CONFIG",
                        RootHive = HKEY.HKEY_CURRENT_CONFIG,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasUnrealizedChildren = true,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    }
                },
            });
        }

        #region Enumerating methods
        public IEnumerable<KeyItem> EnumerateRegistryKeys(HKEY hRootKey, string subRoot, KeyItem parent)
        {
            List<KeyItem> keys = new();
            Win32Error result;

            // Win32API
            result = RVRegOpenKey(hRootKey, subRoot, REGSAM.KEY_READ, out var phKey);
            if (result.Failed)
            {
                Kernel32.SetLastError((uint)result);
                return null;
            }

            // Win32API
            result = RegQueryInfoKey(phKey, null, ref NullRef<uint>(), default, out uint cSubKeys, out uint cbMaxSubKeyLen, out _, out _, out _, out _, out _, out _);
            if (result.Failed)
            {
                return null;
            }

            StringBuilder szName;
            uint cchName;

            for (uint dwIndex = 0; dwIndex < cSubKeys; dwIndex++)
            {
                cchName = cbMaxSubKeyLen + 1;
                szName = new((int)cchName, (int)cchName);

                // Win32API
                result = RegEnumKeyEx(phKey, dwIndex, szName, ref cchName, default, null, ref NullRef<uint>(), out _);
                if (result.Failed)
                {
                    return null;
                }

                var subKeyPath = subRoot == "" ? $"{szName}" : $"{subRoot}\\{szName}";

                result = HasSubKeys(hRootKey, subKeyPath, out var hasChildren);
                if (result.Failed && result != Win32Error.ERROR_ACCESS_DENIED)
                {
                    return null;
                }

                keys.Add(new()
                {
                    Name = szName.ToString(),
                    RootHive = hRootKey,
                    Path = subKeyPath,
                    IsDeletable = true,
                    IsRenamable = true,
                    HasUnrealizedChildren = hasChildren,
                    Image = "ms-appx:///Assets/Images/Folder.png",
                    Parent = parent,
                });
            }

            return keys;
        }

        private Win32Error HasSubKeys(HKEY hRootKey, string subRoot, out bool hasChildren)
        {
            Win32Error result;
            hasChildren = false;

            // Win32API
            result = RVRegOpenKey(hRootKey, subRoot, REGSAM.KEY_READ, out var phKey);
            if (result.Failed)
            {
                Kernel32.SetLastError((uint)result);
                return result;
            }

            // Win32API
            result = RegQueryInfoKey(phKey, null, ref NullRef<uint>(), default, out uint cSubKeys, out _, out _, out _, out _, out _, out _, out _);
            if (result.Failed)
            {
                return result;
            }

            if (cSubKeys == 0)
                hasChildren = false;
            else
                hasChildren = true;

            return Win32Error.ERROR_SUCCESS;
        }
        #endregion

        #region Deleting methods
        private void DeleteSelectedKey(KeyItem key)
        {
            // Run powershell
            bool result = ShellServices.RunPowershellCommand(runAs: true, @$"-command Remove-Item -Path '{key.PathForPwsh}' -Force");
            if (!result)
                return;

            // Remove the key from parent node
            result = key.Parent.Children.Remove(key);
            if (!result)
                return;
        }
        #endregion

        #region Renaming methods
        #endregion
    }
}
