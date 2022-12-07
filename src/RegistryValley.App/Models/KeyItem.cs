namespace RegistryValley.App.Models
{
    public class KeyItem : ObservableObject
    {
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _path;
        public string Path { get => _path; set => SetProperty(ref _path, value); }

        private bool _expanded = false;
        public bool Expanded { get => _expanded; set => SetProperty(ref _expanded, value); }

        private HKEY _rootHive;
        public HKEY RootHive { get => _rootHive; set => SetProperty(ref _rootHive, value); }

        private string _image;
        public string Image { get => _image; set => SetProperty(ref _image, value); }

        public ObservableCollection<KeyItem> Children { get; set; } = new();

        public override string ToString() => Name;
    }
}
