using Microsoft.UI.Xaml;
using RegistryValley.App.Models;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Vanara.Extensions;
using Vanara.InteropServices;
using Vanara.PInvoke;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace RegistryValley.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            _keyItems = new();
            KeyItems = new(_keyItems);

            InitializeHiveTree();
        }

        private readonly ObservableCollection<KeyItem> _keyItems;
        public ReadOnlyObservableCollection<KeyItem> KeyItems { get; }

        private bool _loading;
        public bool Loading { get => _loading; set => SetProperty(ref _loading, value); }

        public void InitializeHiveTree()
        {
            _keyItems.Add(new()
            {
                Name = "Computer",
                Image = "ms-appx:///Assets/Images/Computer.png",
                IsExpanded = true,
                IsSelected = true,
                Children = new()
                {
                    new()
                    {
                        Name = "HKEY_CLASSES_ROOT",
                        RootHive = HKEY.HKEY_CLASSES_ROOT,
                        Path = "",
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_CURRENT_USER",
                        RootHive = HKEY.HKEY_CURRENT_USER,
                        Path = "",
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_LOCAL_MACHINE",
                        RootHive = HKEY.HKEY_LOCAL_MACHINE,
                        Path = "",
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_USERS",
                        RootHive = HKEY.HKEY_USERS,
                        Path = "",
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    },
                    new()
                    {
                        Name = "HKEY_CURRENT_CONFIG",
                        RootHive = HKEY.HKEY_CURRENT_CONFIG,
                        Path = "",
                        Image = "ms-appx:///Assets/Images/Folder.png"
                    }
                },
            });
        }

        public IEnumerable<KeyItem> EnumerateRegistryKeys(HKEY hkey, string subRoot)
        {
            List<KeyItem> keys = new();

            //SetPathItems(hkey, subRoot);

            #region Win32API Calling
            Win32Error result;

            // Win32API
            var handle = RegValleyOpenKey(hkey, subRoot, REGSAM.KEY_READ);
            if (handle != HKEY.NULL)
            {
                return null;
            }

            // Win32API
            result = RegQueryInfoKey(handle, null, ref NullRef<uint>(), default, out uint cKeys, out uint cbMaxSubKeyLen, out _, out _, out _, out _, out _, out _);
            if (result.Failed)
            {
                return null;
            }

            for (uint index = 0; index < cKeys; index++)
            {
                uint cchName = cbMaxSubKeyLen + 1;
                StringBuilder sb = new((int)cchName);

                // Win32API
                result = RegEnumKeyEx(handle, index, sb, ref cchName, default, null, ref NullRef<uint>(), out _);
                if (result.Failed)
                {
                    return null;
                }

                keys.Add(new()
                {
                    Name = sb.ToString(),
                    Path = subRoot + sb.ToString() + "\\",
                    RootHive = hkey,
                    Image = "ms-appx:///Assets/Images/Folder.png",
                });
            }
            #endregion

            return keys;
        }

        private HKEY RegValleyOpenKey(HKEY hkey, string subRoot, REGSAM samDesired, bool use86Arch = false)
        {
            // If specified machine, should use RegConnectRegistry
            var result = RegOpenKeyEx(hkey, subRoot, 0, samDesired, out var phkResult);

            if (result.Succeeded)
                return phkResult;
            else
                return HKEY.NULL;
        }

        unsafe static ref T NullRef<T>()
        {
            return ref Unsafe.AsRef<T>(null);
        }
    }
}
