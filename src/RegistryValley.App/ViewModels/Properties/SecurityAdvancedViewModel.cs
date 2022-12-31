using Microsoft.UI.Xaml;
using RegistryValley.App.Models;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using static RegistryValley.Core.Helpers.RegistryServices;

namespace RegistryValley.App.ViewModels.Properties
{
    public class SecurityAdvancedViewModel : ObservableObject
    {
        public SecurityAdvancedViewModel()
        {
            _principals = new();
            Principals = new(_principals);
        }

        #region Fields and Properties
        private KeyItem _keyItem;
        public KeyItem KeyItem { get => _keyItem; set => SetProperty(ref _keyItem, value); }

        private readonly ObservableCollection<PermissionPrincipalItem> _principals;
        public ReadOnlyObservableCollection<PermissionPrincipalItem> Principals { get; }

        private PermissionPrincipalItem _selectedPrincipal;
        public PermissionPrincipalItem SelectedPrincipal { get => _selectedPrincipal; set => SetProperty(ref _selectedPrincipal, value); }

        private PermissionPrincipalItem _securityDescriptorOwner;
        public PermissionPrincipalItem SecurityDescriptorOwner { get => _securityDescriptorOwner; set => SetProperty(ref _securityDescriptorOwner, value); }

        private bool _hasDacl;
        public bool HasDacl { get => _hasDacl; set => SetProperty(ref _hasDacl, value); }

        private GridLength _columnType = new(36d);
        public GridLength ColumnType { get => _columnType; set => SetProperty(ref _columnType, value); }

        private GridLength _columnEntity = new(200d);
        public GridLength ColumnEntity { get => _columnEntity; set => SetProperty(ref _columnEntity, value); }

        private GridLength _columnAccess = new(160d);
        public GridLength ColumnAccess { get => _columnAccess; set => SetProperty(ref _columnAccess, value); }

        private GridLength _columnInherited = new(70d);
        public GridLength ColumnInherited { get => _columnInherited; set => SetProperty(ref _columnInherited, value); }
        #endregion

        public void LoadKeySecurityOwner()
        {
            if (KeyItem == null)
                return;

            Win32Error result;
            bool bResult;

            result = RVRegOpenKey(KeyItem.RootHive, KeyItem.Path, REGSAM.READ_CONTROL, out var phkResult);

            var pSD = new SafePSECURITY_DESCRIPTOR(512);
            var sdsz = (uint)pSD.Size;

            RegGetKeySecurity(phkResult, SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, pSD, ref sdsz);

            bResult = GetSecurityDescriptorOwner(pSD, out var lpSid, out var bOwnerDefaulted);
            result = Kernel32.GetLastError();

            var cchName = 2048;
            var cchReferencedDomainName = 2048;
            var lpName = new StringBuilder(cchName, cchName);
            var lpReferencedDomainName = new StringBuilder(cchReferencedDomainName, cchReferencedDomainName);

            bResult = LookupAccountSid(null, lpSid, lpName, ref cchName, lpReferencedDomainName, ref cchReferencedDomainName, out var snu);
            result = Kernel32.GetLastError();

            bool isGroup = false;
            bool isUser = false;

            if (snu == SID_NAME_USE.SidTypeAlias || snu == SID_NAME_USE.SidTypeGroup || snu == SID_NAME_USE.SidTypeWellKnownGroup)
                isGroup = true;
            else if (snu == SID_NAME_USE.SidTypeUser)
                isUser = true;

            // Uknown SID type
            if (result.Failed && result == Win32Error.ERROR_NONE_MAPPED)
            {
                // Reset
                lpName.Clear();
                lpReferencedDomainName.Clear();
                isGroup = false;
                isUser = false;
            }

            // Referenced domain name will be used as computer log on name
            if (lpReferencedDomainName.ToString() == "BUILTIN")
            {
                lpReferencedDomainName.Clear();
                lpReferencedDomainName = new(256, 256);
                uint size = (uint)lpReferencedDomainName.Capacity;
                bResult = Kernel32.GetComputerName(lpReferencedDomainName, ref size);
            }
            else
            {
                lpReferencedDomainName.Clear();
            }

            SecurityDescriptorOwner = new()
            {
                SidTypeGlyph = isGroup ? "\xE902" : (isUser ? "\xE2AF" : "\xE716"),
                Name = lpName.ToString(),
                Domain = lpReferencedDomainName.Length == 0 ? "" : lpReferencedDomainName.ToString().ToLower(),
                Sid = lpSid.ToString(),
            };

            lpName.Clear();
            lpReferencedDomainName.Clear();

            pSD.Close();
        }

        public void GetKeyAccessControlList()
        {
            try
            {
                Win32Error result;
                bool bResult;
                var pSD = new SafePSECURITY_DESCRIPTOR(1024);
                var cbSD = (uint)pSD.Size;

                // Win32API
                result = RVRegOpenKey(KeyItem.RootHive, KeyItem.Path, REGSAM.KEY_READ, out var phKey);
                if (result.Failed)
                {
                    Kernel32.SetLastError((uint)result);
                    return;
                }

                // Win32API
                result = RegGetKeySecurity(phKey, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSD, ref cbSD);
                if (result.Failed)
                {
                    Kernel32.SetLastError((uint)result);
                    return;
                }

                // Win32API
                bResult = GetSecurityDescriptorDacl(pSD, out var lpbDaclPresent, out var pDacl, out bool lpbDaclDefaulted);
                result = Kernel32.GetLastError();
                if (result.Failed)
                {
                    Kernel32.SetLastError((uint)result);
                    return;
                }

                if (!lpbDaclPresent)
                {
                    HasDacl = false;
                    return;
                }
                else
                {
                    // Win32API
                    bResult = GetAclInformation(pDacl, out ACL_SIZE_INFORMATION asi, (uint)Marshal.SizeOf(typeof(ACL_SIZE_INFORMATION)), ACL_INFORMATION_CLASS.AclSizeInformation);
                    result = Kernel32.GetLastError();
                    if (result.Failed)
                    {
                        Kernel32.SetLastError((uint)result);
                        return;
                    }

                    HasDacl = true;

                    GetKeyAccessControlEntries(pDacl, asi.AceCount);
                }
            }
            catch
            {
            }
            finally
            {
            }
        }

        private void GetKeyAccessControlEntries(PACL pDacl, uint nAceCount)
        {
            Win32Error result;
            bool bResult;

            SafePSID lpSid;
            AceType aceType;
            ACE_HEADER aceHeader;
            bool aceIsObjectAce;
            ACCESS_MASK accessMask;

            bool isGroup = false;
            bool isUser = false;

            try
            {
                for (var index = 0U; index < nAceCount; index++)
                {
                    // Win32API
                    bResult = GetAce(pDacl, index, out var pAce);
                    result = Kernel32.GetLastError();

                    lpSid = pAce.GetSid();
                    aceType = pAce.GetAceType();
                    aceHeader = pAce.GetHeader();
                    aceIsObjectAce = pAce.IsObjectAce();
                    accessMask = new ACCESS_MASK(pAce.GetMask());

                    // Default inheritance is CIID (CONTAINER_INHERIT_ACE & INHERITED_ACE),
                    // however it is determined by only ID that whether ACE is inherited or not.
                    bool isInherited = aceHeader.AceFlags.HasFlag(AceFlags.InheritOnly);

                    var cchName = 2048;
                    var cchReferencedDomainName = 2048;
                    var lpName = new StringBuilder(cchName, cchName);
                    var lpReferencedDomainName = new StringBuilder(cchReferencedDomainName, cchReferencedDomainName);

                    bResult = LookupAccountSid(null, lpSid, lpName, ref cchName, lpReferencedDomainName, ref cchReferencedDomainName, out var snu);
                    result = Kernel32.GetLastError();

                    if (snu == SID_NAME_USE.SidTypeAlias || snu == SID_NAME_USE.SidTypeGroup || snu == SID_NAME_USE.SidTypeWellKnownGroup)
                        isGroup = true;
                    else if (snu == SID_NAME_USE.SidTypeUser)
                        isUser = true;

                    // Uknown SID type
                    if (result.Failed && result == Win32Error.ERROR_NONE_MAPPED)
                    {
                        // Reset
                        lpName.Clear();
                        lpReferencedDomainName.Clear();
                        isGroup = false;
                        isUser = false;
                    }

                    // Referenced domain name will be used as computer log on name
                    if (lpReferencedDomainName.ToString() == "BUILTIN")
                    {
                        lpReferencedDomainName.Clear();
                        lpReferencedDomainName = new(256, 256);
                        uint size = (uint)lpReferencedDomainName.Capacity;
                        bResult = Kernel32.GetComputerName(lpReferencedDomainName, ref size);
                    }
                    else
                    {
                        lpReferencedDomainName.Clear();
                    }

                    var principal = new PermissionPrincipalItem()
                    {
                        SidTypeGlyph = isGroup ? "\xE902" : (isUser ? "\xE2AF" : "\xE716"),
                        Sid = pAce.GetSid().ToString(),
                        Name = lpName.ToString(),
                        Domain = lpReferencedDomainName.Length == 0 ? "" : lpReferencedDomainName.ToString().ToLower(),
                        AccessControlTypeGlyph = aceType == AceType.AccessAllowed ? "\xE73E" : "\xE711",
                        AccessRuleMerged = new(),
                        AccessRuleAdvanced = new(),
                        HumanizedAccessControlType = aceType == AceType.AccessAllowed ? "Allow" : (aceType == AceType.AccessDenied ? "Deny" : "Unknown")
                    };

                    // TODO: Should merge
                    if (aceType == AceType.AccessAllowed)
                    {
                        principal.AccessRuleMerged = new()
                        {
                            MaskAllowed = accessMask,
                            IsInheritedAllowedMask = isInherited,
                        };
                    }
                    else if (aceType == AceType.AccessDenied)
                    {
                        principal.AccessRuleMerged = new()
                        {
                            MaskDenied = accessMask,
                            IsInheritedDeniedMask = isInherited,
                        };
                    }

                    HumanizeInheritance(principal.AccessRuleAdvanced, aceHeader.AceFlags);

                    _principals.Add(principal);

                    lpName?.Clear();
                    lpReferencedDomainName?.Clear();
                }
            }
            catch
            {
            }
            finally
            {
            }
        }

        private void HumanizeAccessRuleAdvanced(AccessRuleAdvancedItem item)
        {

        }

        private void HumanizeInheritance(AccessRuleAdvancedItem item, AceFlags flags)
        {
            int a = 0;

            if (flags.HasFlag(AceFlags.InheritOnly))
            {
                a++;
            }
            else if (flags.HasFlag(AceFlags.Inherited))
            {
                a++;
            }
            else if (flags.HasFlag(AceFlags.ContainerInherit))
            {
                a++;
            }
        }
    }
}
