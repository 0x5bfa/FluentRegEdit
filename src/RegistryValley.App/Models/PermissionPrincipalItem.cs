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
            => string.IsNullOrEmpty(Name) ? "Unknown Account" : Name;

        public string FullNameOrSid
            => string.IsNullOrEmpty(Name) ? Sid : (string.IsNullOrEmpty(Domain) ? Name : $"{Domain}\\{Name}");

        public AccessRuleAdvancedItem AccessRuleAdvanced { get; set; }

        public AccessRuleMergedItem AccessRuleMerged { get; set; }
    }
}
