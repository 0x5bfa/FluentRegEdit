using Microsoft.UI.Xaml;
using RegistryValley.App.Models;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static Vanara.PInvoke.ComCtl32;
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
                Children = new()
                {
                    new() { Name = "HKEY_CLASSES_ROOT", RootHive = HKEY.HKEY_CLASSES_ROOT, Path = "" },
                    new() { Name = "HKEY_CURRENT_USER", RootHive = HKEY.HKEY_CURRENT_USER, Path = "" },
                    new() { Name = "HKEY_LOCAL_MACHINE", RootHive = HKEY.HKEY_LOCAL_MACHINE, Path = "" },
                    new() { Name = "HKEY_USERS", RootHive = HKEY.HKEY_USERS, Path = "" },
                    new() { Name = "HKEY_CURRENT_CONFIG", RootHive = HKEY.HKEY_CURRENT_CONFIG, Path = "" }
                }
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
                uint cbLen = cbMaxValueLen * 2;
                StringBuilder sb = new((int)cchName);
                IntPtr data = new();

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
                else item.ValueIsString = false;

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
                    switch (item.Type)
                    {
                        case "REG_BINARY":
                            {
                                if ((IntPtr)item.OriginalValue == IntPtr.Zero)
                                {
                                    break;
                                }

                                var value = item.OriginalValue as byte[];
                                if (value.Count() == 0)
                                {
                                    item.FriendlyValue += $"(zero-length binary value)";
                                    break;
                                }

                                foreach (var atom in value)
                                {
                                    item.FriendlyValue += $"{atom} ";
                                }

                                break;
                            }
                        case "REG_MULTI_SZ":
                            {
                                if ((IntPtr)item.OriginalValue  == IntPtr.Zero)
                                {
                                    break;
                                }

                                var value = item.OriginalValue as string[];
                                foreach (var atom in value)
                                {
                                    item.FriendlyValue += $"{atom} ";
                                }

                                break;
                            }
                        case "REZ_EXPAND_SZ":
                        case "REG_SZ":
                            {
                                item.FriendlyValue = item.OriginalValue.ToString();

                                break;
                            }
                        case "REG_QWORD":
                        case "REG_DWORD":
                            {
                                item.FriendlyValue = string.Format(
                                    "0x{0,8:x8} ({1})",
                                    Convert.ToUInt64(item.OriginalValue.ToString()),
                                    item.OriginalValue.ToString()
                                    );

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
