using System.ComponentModel;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace RegistryValley.App.Models
{
    public class AccessRuleItem
    {
        public ACCESS_MASK GrantsAccessMask { get; set; }

        public ACCESS_MASK DeniesAccessMask { get; set; }

        public bool GrantsFullControl
        {
            get => HasFlag(ACCESS_MASK.STANDARD_RIGHTS_ALL, true);
            set => ToggleGrantsPermission(ACCESS_MASK.STANDARD_RIGHTS_ALL, value);
        }

        public bool GrantsRead
        {
            get => HasFlag(ACCESS_MASK.STANDARD_RIGHTS_READ, true);
            set => ToggleGrantsPermission(ACCESS_MASK.STANDARD_RIGHTS_READ, value);
        }

        public bool GrantsSpecial
        {
            get => HasFlag(ACCESS_MASK.SPECIFIC_RIGHTS_ALL, true);
            set => ToggleGrantsPermission(ACCESS_MASK.SPECIFIC_RIGHTS_ALL, value);
        }

        public bool DeniesFullControl
        {
            get => HasFlag(ACCESS_MASK.STANDARD_RIGHTS_ALL, false);
            set => ToggleDeniesPermission(ACCESS_MASK.STANDARD_RIGHTS_ALL, value);
        }

        public bool DeniesRead
        {
            get => HasFlag(ACCESS_MASK.STANDARD_RIGHTS_READ, false);
            set => ToggleDeniesPermission(ACCESS_MASK.STANDARD_RIGHTS_READ, value);
        }

        public bool DeniesSpecial
        {
            get => HasFlag(ACCESS_MASK.SPECIFIC_RIGHTS_ALL, false);
            set => ToggleDeniesPermission(ACCESS_MASK.SPECIFIC_RIGHTS_ALL, value);
        }

        private bool HasFlag(ACCESS_MASK target, bool isGrants = true)
        {
            if (isGrants)
            {
                if ((GrantsAccessMask & target) == target)
                    return true;
                else
                    return false;
            }
            else
            {
                if ((DeniesAccessMask & target) == target)
                    return true;
                else
                    return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ToggleGrantsPermission(ACCESS_MASK permission, bool value)
        {
            if (value && !HasFlag(permission, true))
            {
                GrantsAccessMask |= permission;
            }
            else if (!value && HasFlag(permission, true))
            {
                GrantsAccessMask &= ~permission;
            }

            PropertyChanged?.Invoke(this, new(nameof(GrantsAccessMask)));
        }

        private void ToggleDeniesPermission(ACCESS_MASK permission, bool value)
        {
            if (value && !HasFlag(permission, false))
            {
                DeniesAccessMask |= permission;
            }
            else if (!value && HasFlag(permission, false))
            {
                DeniesAccessMask &= ~permission;
            }

            PropertyChanged?.Invoke(this, new(nameof(DeniesAccessMask)));
        }
    }
}
