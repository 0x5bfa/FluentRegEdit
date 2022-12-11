namespace RegistryValley.App.Models
{
    public class KeyItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

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

        public bool IsSelected { get; set; }

        public bool IsExpanded { get; set; } = false;

        public HKEY RootHive { get; set; }

        public string Image { get; set; }

        public ObservableCollection<KeyItem> Children { get; set; } = new();

        public override string ToString()
            => Name;
    }
}
