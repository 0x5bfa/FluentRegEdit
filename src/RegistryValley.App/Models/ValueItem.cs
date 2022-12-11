namespace RegistryValley.App.Models
{
    public class ValueItem
    {
        public string Name { get; set; }

        public string FriendlyName { get; set; }

        public string Type { get; set; }

        public uint DataSize { get; set; }

        public string FriendlyValue { get; set; }

        public object OriginalValue { get; set; }

        public bool ValueIsString { get; set; }
    }
}
