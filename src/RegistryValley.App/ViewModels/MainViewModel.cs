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

            _valueItems = new();
            ValueItems = new(_valueItems);

            _selectedKeyPathItems = new();
            SelectedKeyPathItems = new(_selectedKeyPathItems);

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

        private readonly ObservableCollection<BreadcrumbBarPathItem> _selectedKeyPathItems;
        public ReadOnlyObservableCollection<BreadcrumbBarPathItem> SelectedKeyPathItems { get; }

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

            SetPathItems(hkey, subRoot);

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

            SetPathItems(hkey, subRoot);

            #region Win32API Calling
            Win32Error result;

            var handle = RegValleyOpenKey(hkey, subRoot, REGSAM.KEY_QUERY_VALUE | REGSAM.READ_CONTROL);

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

            bool hasDefaultKey = false;

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
                    DisplayName = sb.ToString(),
                    TypeString = type.ToString(),
                    DataSize = cbLen,
                    Type = type,
                };

                if (item.TypeString == "REG_DWORD_LITTLE_ENDIAN" || item.TypeString == "REG_DWORD_BIG_ENDIAN")
                    item.TypeString = "REG_DWORD";
                else if (item.TypeString == "REG_QWORD_LITTLE_ENDIAN")
                    item.TypeString = "REG_QWORD";

                if (item.TypeString == "REG_SZ" || item.TypeString == "REG_EXPAND_SZ" || item.TypeString == "REG_MULTI_SZ")
                    item.IsStringBased = true;
                else
                    item.IsNumericalBased = true;

                if (string.IsNullOrEmpty(item.Name))
                {
                    item.DisplayName = "(Default)";
                    item.IsRenamable = false;
                    item.DisplayValue = data.ToString(-1, CharSet.Auto);
                    item.EditableValue = "";
                    item.IsString = true;
                    hasDefaultKey = true;

                    data.Close();
                    _valueItems.Add(item);
                    continue;
                }

                switch (type)
                {
                    case REG_VALUE_TYPE.REG_BINARY:
                        {
                            item.IsBinary = true;

                            var value = data.ToStructure<byte[]>();
                            value = value.Take((int)item.DataSize).ToArray();

                            if (value.Length == 0)
                            {
                                item.DisplayValue = $"(zero-length binary value)";
                                item.EditableValue = "";
                                break;
                            }

                            foreach (var atom in value)
                            {
                                item.DisplayValue += string.Format("{0,2:x2} ", Convert.ToUInt32(atom));
                            }

                            item.DisplayValue = item.DisplayValue.TrimEnd();
                            item.EditableValue = item.DisplayValue;
                        }
                        break;

                    case REG_VALUE_TYPE.REG_MULTI_SZ:
                        {
                            item.IsMultiString = true;

                            var value = data.ToString(-1, CharSet.Auto);

                            foreach (var atom in value.Split('\n'))
                            {
                                item.DisplayValue += $"{atom} ";
                            }

                            item.DisplayValue = item.DisplayValue.TrimEnd();
                            item.EditableValue = value;
                        }
                        break;

                    case REG_VALUE_TYPE.REG_EXPAND_SZ:
                    case REG_VALUE_TYPE.REG_SZ:
                        {
                            item.IsString = true;

                            var value = data.ToString(-1, CharSet.Auto);

                            item.DisplayValue = value;
                            item.EditableValue = item.DisplayValue;
                        }
                        break;

                    case REG_VALUE_TYPE.REG_QWORD:
                    case REG_VALUE_TYPE.REG_DWORD:
                        {
                            item.IsDwordOrQword = true;

                            var value = data.ToStructure<uint>();

                            item.DisplayValue = string.Format("0x{0,8:x8} ({1})", Convert.ToUInt32(value), Convert.ToUInt32(value));
                            item.EditableValue = Convert.ToUInt32(value).ToString();
                        }
                        break;
                }

                _valueItems.Add(item);

                data.Close();
            }

            if (!hasDefaultKey)
            {
                _valueItems.Insert(0, new()
                {
                    Name = "",
                    DataSize = 0,
                    Type = REG_VALUE_TYPE.REG_SZ,

                    DisplayName = "(Default)",
                    TypeString = "REG_SZ",
                    DisplayValue = "(Value not set)",
                    EditableValue = "",

                    IsRenamable = false,
                    IsString = true,
                    IsStringBased = true,
                });
            }
            #endregion

            // Order
            var alphabetic = new ObservableCollection<ValueItem>(_valueItems.OrderBy(x => x.DisplayName));
            _valueItems.Clear();
            foreach (var item in alphabetic)
                _valueItems.Add(item);
        }

        public void SetPathItems(HKEY hkey, string subRoot)
        {
            _selectedKeyPathItems.Clear();

            _selectedKeyPathItems.Add(new() { PathItem = "Computer" });

            if (hkey == HKEY.HKEY_CLASSES_ROOT)
                _selectedKeyPathItems.Add(new() { PathItem = "HKEY_CLASSES_ROOT" });
            else if (hkey == HKEY.HKEY_CURRENT_CONFIG)
                _selectedKeyPathItems.Add(new() { PathItem = "HKEY_CURRENT_CONFIG" });
            else if (hkey == HKEY.HKEY_CURRENT_USER)
                _selectedKeyPathItems.Add(new() { PathItem = "HKEY_CURRENT_USER" });
            else if (hkey == HKEY.HKEY_LOCAL_MACHINE)
                _selectedKeyPathItems.Add(new() { PathItem = "HKEY_LOCAL_MACHINE" });
            else if (hkey == HKEY.HKEY_USERS)
                _selectedKeyPathItems.Add(new() { PathItem = "HKEY_USERS" });

            if (string.IsNullOrEmpty(subRoot) || subRoot.Split('\\').Length == 0)
            {
                _selectedKeyPathItems[^1].IsLast = true;
                return;
            }

            subRoot = subRoot.TrimEnd('\\');
            var items = subRoot.Split('\\');

            foreach (var item in items)
            {
                _selectedKeyPathItems.Add(new() { PathItem = item });
            }

            _selectedKeyPathItems[^1].IsLast = true;
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

        public void ClearValueItems()
            => _valueItems.Clear();

        public void ClearSelectedKeyPathItems()
            => _selectedKeyPathItems.Clear();

        public void AddItemToSelectedKeyPathItems(BreadcrumbBarPathItem item)
            => _selectedKeyPathItems.Add(item);
    }
}
