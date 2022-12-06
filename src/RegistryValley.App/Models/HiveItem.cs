using Microsoft.Win32;

namespace RegistryValley.App.Models
{
    public class HiveItem : ObservableObject
    {
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _path;
        public string Path { get => _path; set => SetProperty(ref _path, value); }

        private bool _expanded = false;
        public bool Expanded { get => _expanded; set => SetProperty(ref _expanded, value); }

        private RegistryHive _hive;
        public RegistryHive Hive { get => _hive; set => SetProperty(ref _hive, value); }

        private string _image;
        public string Image { get => _image; set => SetProperty(ref _image, value); }

        public ObservableCollection<HiveItem> Children { get; set; } = new();

        public override string ToString()
            => Name;
    }
}
