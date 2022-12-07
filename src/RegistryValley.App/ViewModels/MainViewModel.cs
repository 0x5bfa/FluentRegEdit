using Microsoft.UI.Xaml;
using RegistryValley.App.Models;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Input;

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

        private GridLength _columnName;
        public GridLength ColumnName { get => _columnName; set => SetProperty(ref _columnName, value); }

        private GridLength _columnType;
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

            Win32Error result;

            var handle = RegValleyOpenKey(hkey, subRoot, REGSAM.KEY_READ);

            result = RegQueryInfoKey(
                handle,
                null,
                ref NullRef<uint>(),
                IntPtr.Zero,
                out uint nKeys,
                out uint dwMaxKeyNameSize,
                out NullRef<uint>(),
                out uint nValues,
                out uint dwMaxValueNameSize,
                out NullRef<uint>(),
                out NullRef<uint>(),
                out NullRef<System.Runtime.InteropServices.ComTypes.FILETIME>()
                );

            for (uint index = 0; index < nKeys; index++)
            {
                StringBuilder sb = new((int)dwMaxKeyNameSize);

                uint cchName = dwMaxKeyNameSize;

                result = RegEnumKeyEx(
                    handle,
                    index,
                    sb,
                    ref cchName,
                    IntPtr.Zero,
                    null,
                    ref NullRef<uint>(),
                    out NullRef<System.Runtime.InteropServices.ComTypes.FILETIME>());

                keys.Add(new()
                {
                    Name = sb.ToString(),
                    Path = subRoot + sb.ToString() + "\\"
                });
            }

            return keys;
        }

        public void RegValleyEnumValues(HKEY hkey, string subRoot, string keyName)
        {
            _valueItems.Clear();

            Win32Error result;

            var handle = RegValleyOpenKey(hkey, subRoot, REGSAM.KEY_READ);

            result = RegQueryInfoKey(
                handle,
                null,
                ref NullRef<uint>(),
                IntPtr.Zero,
                out uint nKeys,
                out uint dwMaxKeyNameSize,
                out NullRef<uint>(),
                out uint nValues,
                out uint dwMaxValueNameSize,
                out uint dwMaxValueDataSize,
                out NullRef<uint>(),
                out NullRef<System.Runtime.InteropServices.ComTypes.FILETIME>()
                );

            for (uint index = 0; index < nKeys; index++)
            {
                StringBuilder sb = new((int)dwMaxKeyNameSize);
                IntPtr data = IntPtr.Zero;

                uint cchName = dwMaxValueNameSize;

                result = RegEnumValue(
                    handle,
                    index,
                    sb,
                    ref cchName,
                    IntPtr.Zero,
                    out var type,
                    data,
                    ref dwMaxValueDataSize);

                _valueItems.Add(new()
                {
                    Name = sb.ToString(),
                    FriendlyName = sb.ToString(),
                    Type = type.ToString(),
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
                return null;
        }

        unsafe static ref T NullRef<T>()
        {
            return ref Unsafe.AsRef<T>(null);
        }
    }
}
