namespace RegistryValley.App.Models
{
    public class PermissionPrincipalItem
    {
        public string SidTypeGlyph { get; set; }

        public string Sid { get; set; }

        public string Name { get; set; }

        public string Domain { get; set; }

        public string AccessControlTypeGlyph { get; set; }

        public string DisplayName
            => string.IsNullOrEmpty(Name) ? Sid : Name;

        public string FullName
            => string.IsNullOrEmpty(Domain) ? "" : $"{Domain}\\{Name}";

        public string HumanizedAccessControl { get; set; }

        public string HumanizedInheritance { get; set; }

        public string HumanizedAccessControlType { get; set; }

        public AccessRuleAdvancedItem AccessRuleAdvanced { get; set; }

        public AccessRuleMergedItem AccessRuleMerged { get; set; }
    }
}
