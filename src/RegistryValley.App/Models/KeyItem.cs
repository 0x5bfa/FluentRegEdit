namespace RegistryValley.App.Models
{
    public class KeyItem : ObservableObject
    {
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _path;
        public string Path { get => _path; set => SetProperty(ref _path, value); }

        public string DisplayPath
        {
            get
            {
                string path = Path;
                string displayPath;

                if (string.IsNullOrEmpty(Path))
                {
                    if (RootHive == HKEY.HKEY_CLASSES_ROOT)
                        displayPath = $"HKEY_CLASSES_ROOT";
                    else if (RootHive == HKEY.HKEY_CURRENT_CONFIG)
                        displayPath = $"HKEY_CURRENT_CONFIG";
                    else if (RootHive == HKEY.HKEY_CURRENT_USER)
                        displayPath = $"HKEY_CURRENT_USER";
                    else if (RootHive == HKEY.HKEY_LOCAL_MACHINE)
                        displayPath = $"HKEY_LOCAL_MACHINE";
                    else if (RootHive == HKEY.HKEY_USERS)
                        displayPath = $"HKEY_USERS";
                    else
                        displayPath = $"No path";
                }
                else
                {
                    path = Path.TrimEnd('\\');

                    if (RootHive == HKEY.HKEY_CLASSES_ROOT)
                        displayPath = $"HKCR:\\{path}";
                    else if (RootHive == HKEY.HKEY_CURRENT_CONFIG)
                        displayPath = $"HKCC:\\{path}";
                    else if (RootHive == HKEY.HKEY_CURRENT_USER)
                        displayPath = $"HKCU:\\{path}";
                    else if (RootHive == HKEY.HKEY_LOCAL_MACHINE)
                        displayPath = $"HKLM:\\{path}";
                    else if (RootHive == HKEY.HKEY_USERS)
                        displayPath = $"HKU:\\{path}";
                    else
                        displayPath = $"{path}";
                }

                return displayPath;
            }
        }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

        private bool _isExpanded;
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }

        private HKEY _rootHive;
        public HKEY RootHive { get => _rootHive; set => SetProperty(ref _rootHive, value); }

        private string _image;
        public string Image { get => _image; set => SetProperty(ref _image, value); }

        public ObservableCollection<KeyItem> Children { get; set; } = new();

        public override string ToString()
            => Name;
    }
}
