using Microsoft.UI.Xaml;
using RegistryValley.App.Models;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Vanara.Extensions;
using Vanara.InteropServices;
using static Vanara.PInvoke.ComCtl32;
using static Vanara.PInvoke.User32.RAWINPUT;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace RegistryValley.App.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            _keyItems = new();
            KeyItems = new(_keyItems);

            _valueItems = new();
            ValueItems = new(_valueItems);

            InitializeHiveTree();
        }

        private readonly ObservableCollection<KeyItem> _keyItems;
        public ReadOnlyObservableCollection<KeyItem> KeyItems { get; }

        private readonly ObservableCollection<ValueItem> _valueItems;
        public ReadOnlyObservableCollection<ValueItem> ValueItems { get; }

        private GridLength _columnName = new(256d);
        public GridLength ColumnName { get => _columnName; set => SetProperty(ref _columnName, value); }

        private GridLength _columnType = new(144d);
        public GridLength ColumnType { get => _columnType; set => SetProperty(ref _columnType, value); }

        private bool _loading;
        public bool Loading { get => _loading; set => SetProperty(ref _loading, value); }

        public void InitializeHiveTree()
        {
            _keyItems.Add(new()
            {
                Name = "Computer",
                IsExpanded = true,
                Image = "ms-appx:///Assets/Images/Computer.png",
                Children = new()
                {
                    new() { Name = "HKEY_CLASSES_ROOT", RootHive = HKEY.HKEY_CLASSES_ROOT, Path = "", Image = "ms-appx:///Assets/Images/Folder.png" },
                    new() { Name = "HKEY_CURRENT_USER", RootHive = HKEY.HKEY_CURRENT_USER, Path = "", Image = "ms-appx:///Assets/Images/Folder.png" },
                    new() { Name = "HKEY_LOCAL_MACHINE", RootHive = HKEY.HKEY_LOCAL_MACHINE, Path = "", Image = "ms-appx:///Assets/Images/Folder.png" },
                    new() { Name = "HKEY_USERS", RootHive = HKEY.HKEY_USERS, Path = "", Image = "ms-appx:///Assets/Images/Folder.png" },
                    new() { Name = "HKEY_CURRENT_CONFIG", RootHive = HKEY.HKEY_CURRENT_CONFIG, Path = "", Image = "ms-appx:///Assets/Images/Folder.png" }
                },
            });
        }

        public IEnumerable<KeyItem> RegValleyEnumKeys(HKEY hkey, string subRoot)
        {
            List<KeyItem> keys = new();

            #region Win32API Calling
            Win32Error result;

            var handle = RegValleyOpenKey(hkey, subRoot, REGSAM.KEY_READ);

            result = RegQueryInfoKey(
                handle,
                null,
                ref NullRef<uint>(),
                IntPtr.Zero,
                out uint cKeys,
                out uint cbMaxSubKeyLen,
                out NullRef<uint>(),
                out NullRef<uint>(),
                out NullRef<uint>(),
                out NullRef<uint>(),
                out NullRef<uint>(),
                out NullRef<FILETIME>()
                );

            for (uint index = 0; index < cKeys; index++)
            {
                uint cchName = cbMaxSubKeyLen + 1;
                StringBuilder sb = new((int)cchName);

                result = RegEnumKeyEx(
                    handle,
                    index,
                    sb,
                    ref cchName,
                    IntPtr.Zero,
                    null,
                    ref NullRef<uint>(),
                    out NullRef<FILETIME>());

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

        public void RegValleyEnumValues(HKEY hkey, string subRoot, string keyName)
        {
            _valueItems.Clear();

            #region Win32API Calling
            Win32Error result;

            var handle = RegValleyOpenKey(hkey, subRoot, REGSAM.KEY_READ | REGSAM.KEY_ENUMERATE_SUB_KEYS);

            result = RegQueryInfoKey(
                handle,
                null,
                ref NullRef<uint>(),
                IntPtr.Zero,
                out NullRef<uint>(),
                out NullRef<uint>(),
                out NullRef<uint>(),
                out uint cValues,
                out uint cbMaxValueNameLen,
                out uint cbMaxValueLen,
                out NullRef<uint>(),
                out NullRef<FILETIME>()
                );

            for (uint index = 0; index < cValues; index++)
            {
                uint cchName = cbMaxValueNameLen + 1;
                StringBuilder sb = new((int)cchName);
                uint cbLen = cbMaxValueLen + (cbMaxValueLen % 2);
                var data = new SafeHGlobalHandle(cbLen);

                result = RegEnumValue(
                    handle,
                    index,
                    sb,
                    ref cchName,
                    IntPtr.Zero,
                    out var type,
                    data,
                    ref cbLen);

                ValueItem item = new()
                {
                    Name = sb.ToString(),
                    FriendlyName = sb.ToString(),
                    Type = type.ToString(),
                    OriginalValue = data,
                };

                item.Type = item.Type == "REG_DWORD_LITTLE_ENDIAN" ? "REG_DWORD" : item.Type;
                item.Type = item.Type == "REG_DWORD_BIG_ENDIAN" ? "REG_DWORD" : item.Type;
                item.Type = item.Type == "REG_QWORD_LITTLE_ENDIAN" ? "REG_QWORD" : item.Type;

                if (item.Type == "REG_SZ" ||
                    item.Type == "REG_EXPAND_SZ" ||
                    item.Type == "REG_MULTI_SZ")
                {
                    item.ValueIsString = true;
                }
                else
                    item.ValueIsString = false;

                _valueItems.Add(item);
            }
            #endregion

            NormalizeValues();
        }

        private void NormalizeValues()
        {
            bool hasSetDefaultKey = false;
            int index = 0;

            foreach (var item in ValueItems)
            {
                if (string.IsNullOrEmpty(item.Name))
                {
                    hasSetDefaultKey = true;
                    item.FriendlyName = "(Default)";
                    item.FriendlyValue = item.OriginalValue.ToString();
                }
                else
                {
                    var val = (SafeHGlobalHandle)item.OriginalValue;

                    switch (item.Type)
                    {
                        case "REG_BINARY":
                            {
                                var value = val.ToStructure<uint>();
                                var byteVal = BitConverter.GetBytes(value);

                                if (byteVal.Length == 0)
                                {
                                    item.FriendlyValue += $"(zero-length binary value)";
                                    break;
                                }

                                foreach (var atom in byteVal)
                                {
                                    item.FriendlyValue += $"{atom} ";
                                }

                                break;
                            }
                        case "REG_MULTI_SZ":
                            {
                                var value = val.ToString();

                                foreach (var atom in value.Split('\n'))
                                {
                                    item.FriendlyValue += $"{atom} ";
                                }

                                break;
                            }
                        case "REZ_EXPAND_SZ":
                        case "REG_SZ":
                            {
                                var value = val.ToString();

                                item.FriendlyValue = value;

                                break;
                            }
                        case "REG_QWORD":
                        case "REG_DWORD":
                            {
                                var value = val.ToStructure<uint>();

                                item.FriendlyValue = string.Format("0x{0,8:x8} ({1})", Convert.ToUInt32(value), Convert.ToUInt32(value));

                                break;
                            }
                    }

                    if (!hasSetDefaultKey)
                    {
                        // Indedx of empty name
                        index++;
                    }
                }
            }

            if (hasSetDefaultKey)
            {
                _valueItems.Move(index, 0);
            }
            else
            {
                _valueItems.Insert(0, new()
                {
                    FriendlyName = "(Default)",
                    FriendlyValue = "(Value not set)",
                    Type = "REG_SZ",
                    ValueIsString = true,
                });
            }
        }

        HKEY RegValleyOpenKey(HKEY hkey, string subRoot, REGSAM samDesired, bool use86Arch = false)
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
