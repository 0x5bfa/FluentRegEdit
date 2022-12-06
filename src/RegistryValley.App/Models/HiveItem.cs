using Microsoft.Win32;
using System.ComponentModel;

namespace RegistryValley.App.Models
{
    public class HiveItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        private bool _expanded = false;
        public bool Expanded { get; set; }

        public RegistryHive Hive { get; set; }

        public string Image { get; set; }

        public ObservableCollection<HiveItem> Children { get; set; } = new();

        public override string ToString()
            => Name;
    }
}
