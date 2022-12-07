namespace RegistryValley.App.Models
{
    public class KeyItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public bool Expanded { get; set; }

        public HKEY RootHive { get; set; }

        public string Image { get; set; }

        public ObservableCollection<KeyItem> Children { get; set; } = new();

        public override string ToString() => Name;
    }
}
