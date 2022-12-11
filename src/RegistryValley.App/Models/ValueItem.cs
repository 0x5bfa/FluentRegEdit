namespace RegistryValley.App.Models
{
    public class ValueItem
    {
        public string Name { get; set; }

        public uint DataSize { get; set; }

        public REG_VALUE_TYPE Type { get; set; } = REG_VALUE_TYPE.REG_NONE;


        public string DisplayName { get; set; }

        public string TypeString { get; set; }

        public string DisplayValue { get; set; }

        public string EditableValue { get; set; }


        public bool IsRenamable { get; set; } = true;

        public bool DataIsEditable { get; set; } = true;


        public bool IsBinary { get; set; }

        public bool IsDwordOrQword { get; set; }

        public bool IsString { get; set; }

        public bool IsMultiString { get; set; }


        public bool IsStringBased { get; set; }

        public bool IsNumericalBased { get; set; }
    }
}
