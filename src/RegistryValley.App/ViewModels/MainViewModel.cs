using RegistryValley.App.Services;
using RegistryValley.App.Models;

namespace RegistryValley.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            _keyItems = new();
            KeyItems = new(_keyItems);

            _flatKeyItems = new();
            FlatKeyItems = new(_flatKeyItems);

            DeleteKeyCommand = new RelayCommand<KeyItem>(DeleteSelectedKey);
            RenameKeyCommand = new RelayCommand<KeyItem>(RenameSelectedKey);

            InitializeHiveTree();
        }

        #region Fields and Properties
        private readonly ObservableCollection<KeyItem> _keyItems;
        public ReadOnlyObservableCollection<KeyItem> KeyItems { get; }

        private readonly ObservableCollection<KeyItem> _flatKeyItems;
        public ReadOnlyObservableCollection<KeyItem> FlatKeyItems { get; }

        public string LastRenamedNewName { get; set; }

        public bool CreatingNewKey { get; set; }

        public IRelayCommand DeleteKeyCommand { get; }
        public IRelayCommand RenameKeyCommand { get; }
        #endregion

        private void InitializeHiveTree()
        {
            _keyItems.Add(new()
            {
                Name = "Computer",
                RootHive = HKEY.NULL,
                IsExpanded = true,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasChildren = true,
                SelectedRootComputer = true,
                Image = "ms-appx:///Assets/Images/Computer.png",
                Depth = 1,
                Children = new()
                {
                    new()
                    {
                        Name = "HKEY_CLASSES_ROOT",
                        RootHive = HKEY.HKEY_CLASSES_ROOT,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasChildren = true,
                        Depth = 2,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_CURRENT_USER",
                        RootHive = HKEY.HKEY_CURRENT_USER,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasChildren = true,
                        Depth = 2,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_LOCAL_MACHINE",
                        RootHive = HKEY.HKEY_LOCAL_MACHINE,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasChildren = true,
                        Depth = 2,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_USERS",
                        RootHive = HKEY.HKEY_USERS,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasChildren = true,
                        Depth = 2,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_CURRENT_CONFIG",
                        RootHive = HKEY.HKEY_CURRENT_CONFIG,
                        Path = "",
                        IsDeletable = false,
                        IsRenamable = false,
                        HasChildren = true,
                        Depth = 2,
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    }
                },
            });

            _flatKeyItems.Add(new()
            {
                Name = "Computer",
                RootHive = HKEY.NULL,
                IsExpanded = true,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasChildren = true,
                SelectedRootComputer = true,
                Depth = 1,
                Image = "ms-appx:///Assets/Images/Computer.png",
            });
            _flatKeyItems.Add(new()
            {
                Name = "HKEY_CLASSES_ROOT",
                RootHive = HKEY.HKEY_CLASSES_ROOT,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasChildren = true,
                Depth = 2,
                Image = "ms-appx:///Assets/Images/Folder.png"
            });
            _flatKeyItems.Add(new()
            {
                Name = "HKEY_CURRENT_USER",
                RootHive = HKEY.HKEY_CURRENT_USER,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasChildren = true,
                Depth = 2,
                Image = "ms-appx:///Assets/Images/Folder.png"
            });
            _flatKeyItems.Add(new()
            {
                Name = "HKEY_LOCAL_MACHINE",
                RootHive = HKEY.HKEY_LOCAL_MACHINE,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasChildren = true,
                Depth = 2,
                Image = "ms-appx:///Assets/Images/Folder.png"
            });
            _flatKeyItems.Add(new()
            {
                Name = "HKEY_USERS",
                RootHive = HKEY.HKEY_USERS,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasChildren = true,
                Depth = 2,
                Image = "ms-appx:///Assets/Images/Folder.png"
            });
            _flatKeyItems.Add(new()
            {
                Name = "HKEY_CURRENT_CONFIG",
                RootHive = HKEY.HKEY_CURRENT_CONFIG,
                Path = "",
                IsDeletable = false,
                IsRenamable = false,
                HasChildren = true,
                Depth = 2,
                Image = "ms-appx:///Assets/Images/Folder.png"
            });

        }

        #region Enumerating methods
        public IEnumerable<KeyItem> EnumerateRegistryKeys(HKEY hRootKey, string subRoot, KeyItem parent)
        {
            List<KeyItem> keys = new();
            Win32Error result;

            // Calling Win32API
            result = RegOpenKeyEx(hRootKey, subRoot, 0, REGSAM.KEY_READ, out var phKey);
            if (result.Failed)
            {
                Kernel32.SetLastError((uint)result);
                result.ThrowIfFailed();
                return null;
            }

            // Calling Win32API
            result = RegQueryInfoKey(phKey, null, ref NullRef<uint>(), default, out uint cSubKeys, out uint cbMaxSubKeyLen, out _, out _, out _, out _, out _, out _);
            if (result.Failed)
            {
                Kernel32.SetLastError((uint)result);
                result.ThrowIfFailed();
                return null;
            }

            StringBuilder szName;
            uint cchName;

            for (uint dwIndex = 0; dwIndex < cSubKeys; dwIndex++)
            {
                cchName = cbMaxSubKeyLen + 1;
                szName = new((int)cchName, (int)cchName);

                // Calling Win32API
                result = RegEnumKeyEx(phKey, dwIndex, szName, ref cchName, default, null, ref NullRef<uint>(), out _);
                if (result.Failed)
                {
                    Kernel32.SetLastError((uint)result);
                    result.ThrowIfFailed();
                    return null;
                }

                var subKeyPath = subRoot == "" ? $"{szName}" : $"{subRoot}\\{szName}";

                // Calling Win32API
                result = HasSubKeys(hRootKey, subKeyPath, out var hasChildren);
                if (result.Failed && result != Win32Error.ERROR_ACCESS_DENIED)
                {
                    Kernel32.SetLastError((uint)result);
                    result.ThrowIfFailed();
                    return null;
                }

                keys.Add(new()
                {
                    Name = szName.ToString(),
                    RootHive = hRootKey,
                    Path = subKeyPath,
                    IsDeletable = true,
                    IsRenamable = true,
                    HasChildren = hasChildren,
                    Image = "ms-appx:///Assets/Images/Folder.png",
                    Depth = parent.Depth + 1,
                    Parent = parent,
                });
            }

            return keys;
        }

        private Win32Error HasSubKeys(HKEY hRootKey, string subRoot, out bool hasChildren)
        {
            Win32Error result;
            hasChildren = false;

            // Calling Win32API
            result = RegOpenKeyEx(hRootKey, subRoot, 0, REGSAM.KEY_READ, out var phKey);
            if (result.Failed)
            {
                return result;
            }

            // Calling Win32API
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
            var result = ShellServices.RunPowershellCommand(runAs: true, @$"-command Remove-Item -Path '{key.PathForPwsh}' -Recurse");

            key.Parent.Children.Remove(key);
            _flatKeyItems.Remove(key);

            if (key.Parent.Children.Count == 0)
                key.Parent.HasChildren = false;
        }
        #endregion

        #region Renaming methods
        private void RenameSelectedKey(KeyItem key)
        {
            key.IsRenaming = false;
            key.Name = LastRenamedNewName;

            var pathItems = key.PathForPwsh.Split('\\').ToList();
            pathItems.RemoveAt(pathItems.Count - 1);
            var parentPath = string.Join('\\', pathItems);

            var command = @$"-command if (!(Test-Path '{key.PathForPwsh}')) {{ New-Item -Path '{parentPath}' -Name '{key.Name}' -Force }} else {{ Rename-Item '{key.PathForPwsh}' -NewName '{LastRenamedNewName}' }}";
            var result = ShellServices.RunPowershellCommand(runAs: true, command);
        }
        #endregion

        #region Flat Collection Handling methods
        public void Insert(int index, KeyItem item)
        {
            _flatKeyItems.Insert(index, item);
        }

        public void RemoveAll(int depth, int startIndex)
        {
            int lastRemovedItemIndex = 0;
            var list = _flatKeyItems.Where(x => x.Depth > depth && _flatKeyItems.IndexOf(x) > startIndex).ToList();
            lastRemovedItemIndex = _flatKeyItems.IndexOf(list.FirstOrDefault());

            foreach (var item in list)
            {
                if (lastRemovedItemIndex == _flatKeyItems.IndexOf(item))
                    _flatKeyItems.Remove(item);
                else
                    break;
            }
        }
        #endregion
    }
}
