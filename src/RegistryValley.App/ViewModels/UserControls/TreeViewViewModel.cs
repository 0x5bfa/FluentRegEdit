using RegistryValley.App.Services;
using RegistryValley.App.Models;

namespace RegistryValley.App.ViewModels.UserControls
{
    public class TreeViewViewModel
    {
        public TreeViewViewModel()
        {
            KeyItems = new();
            FlatKeyItems = new();

            KeyItems = Data.DefaultKeyItemFactory.DefaultNestedKeyTree;
            FlatKeyItems = Data.DefaultKeyItemFactory.DefaultFlattenedKeyTree;
        }

        #region Fields and Properties
        public ObservableCollection<KeyItem> KeyItems { get; }
        public ObservableCollection<KeyItem> FlatKeyItems { get; }

        public string LastRenamedNewName { get; set; }
        public bool CreatingNewKey { get; set; }

        public IRelayCommand DeleteKeyCommand { get; }
        public IRelayCommand RenameKeyCommand { get; }
        #endregion

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
    }
}
