using RegistryValley.App.Models;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace RegistryValley.App.ViewModels.Dialogs
{
    public class KeyPermissionsViewerDialogViewModel : ObservableObject
    {
        public KeyPermissionsViewerDialogViewModel()
        {
            _principalsMerged = new();
            PrincipalsMerged = new(_principalsMerged);

            _principalsAdvanced = new();
            PrincipalsAdvanced = new(_principalsAdvanced);

            LoadKeySecurityCommand = new RelayCommand(LoadKeySecurity);
        }

        #region Fields and Properties
        private KeyItem _keyItem;
        public KeyItem KeyItem { get => _keyItem; set => SetProperty(ref _keyItem, value); }

        private readonly ObservableCollection<PermissionPrincipalItem> _principalsMerged;
        public ReadOnlyObservableCollection<PermissionPrincipalItem> PrincipalsMerged { get; }

        private readonly ObservableCollection<PermissionPrincipalItem> _principalsAdvanced;
        public ReadOnlyObservableCollection<PermissionPrincipalItem> PrincipalsAdvanced { get; }

        private PermissionPrincipalItem _selectedPrincipal;
        public PermissionPrincipalItem SelectedPrincipal { get => _selectedPrincipal; set => SetProperty(ref _selectedPrincipal, value); }

        private PermissionPrincipalItem _securityDescriptorOwner;
        public PermissionPrincipalItem SecurityDescriptorOwner { get => _securityDescriptorOwner; set => SetProperty(ref _securityDescriptorOwner, value); }

        private bool _hasDacl;
        public bool HasDacl { get => _hasDacl; set => SetProperty(ref _hasDacl, value); }

        private bool _showSecurityAdvanced;
        public bool ShowAdvancedPermissions { get => _showSecurityAdvanced; set => SetProperty(ref _showSecurityAdvanced, value); }

        private bool _selectedAdvancedPermissionItem;
        public bool SelectedAdvancedPermissionItem { get => _selectedAdvancedPermissionItem; set => SetProperty(ref _selectedAdvancedPermissionItem, value); }

        private PermissionPrincipalItem _selectedPrincipalAdvancedPermission;
        public PermissionPrincipalItem SelectedPrincipalAdvancedPermission { get => _selectedPrincipalAdvancedPermission; set => SetProperty(ref _selectedPrincipalAdvancedPermission, value); }

        public IRelayCommand LoadKeySecurityCommand { get; }
        #endregion

        private void LoadKeySecurity()
        {
            LoadKeySecurityOwner();

            GetKeyAccessControlList();
        }

        public void GetKeyAccessControlList()
        {
            try
            {
                Win32Error result;
                bool bResult;

                var hkey = RVRegOpenKey(KeyItem.RootHive, KeyItem.Path, REGSAM.READ_CONTROL);

                var pSD = new SafePSECURITY_DESCRIPTOR(1024);
                var cbSD = (uint)pSD.Size;

                result = RegGetKeySecurity(hkey, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSD, ref cbSD);

                bResult = GetSecurityDescriptorDacl(pSD, out var lpbDaclPresent, out var pDacl, out bool lpbDaclDefaulted);
                result = Kernel32.GetLastError();

                if (!lpbDaclPresent)
                {
                    HasDacl = false;
                }
                else
                {
                    bResult = GetAclInformation(pDacl, out ACL_SIZE_INFORMATION asi, (uint)Marshal.SizeOf(typeof(ACL_SIZE_INFORMATION)), ACL_INFORMATION_CLASS.AclSizeInformation);
                    result = Kernel32.GetLastError();

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

                    var principal = new PermissionPrincipalItem()
                    {
                        SidTypeGlyph = isGroup ? "\xE902" : (isUser ? "\xE2AF" : "\xE716"),
                        Sid = pAce.GetSid().ToString(),
                        Name = lpName.ToString(),
                        Domain = lpReferencedDomainName.ToString(),
                        AccessControlTypeGlyph = aceType == AceType.AccessAllowed ? "\xE73E" : "\xE711",
                        AccessRuleMerged = new(),
                        AccessRuleAdvanced = new()
                    };

                    _principalsMerged.Add(principal);
                    _principalsAdvanced.Add(principal);

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

        private void GetAdvancedAccessRules()
        {
        }

        private void GetMergedAccessRules()
        {

        }

        public void LoadKeySecurityOwner()
        {
            if (KeyItem == null)
                return;

            Win32Error result;
            bool bResult;

            var hkey = RVRegOpenKey(KeyItem.RootHive, KeyItem.Path, REGSAM.READ_CONTROL);

            var pSD = new SafePSECURITY_DESCRIPTOR(512);
            var sdsz = (uint)pSD.Size;

            RegGetKeySecurity(hkey, SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, pSD, ref sdsz);

            bResult = GetSecurityDescriptorOwner(pSD, out var owner, out var bOwnerDefaulted);
            result = Kernel32.GetLastError();

            var accountSize = 2048;
            var domainSize = 2048;
            var outuser = new StringBuilder(accountSize, accountSize);
            var outdomain = new StringBuilder(domainSize, domainSize);

            bResult = LookupAccountSid(null, owner, outuser, ref accountSize, outdomain, ref domainSize, out var snu);
            result = Kernel32.GetLastError();

            bool isGroup = false;
            bool isUser = false;

            if (snu == SID_NAME_USE.SidTypeAlias || snu == SID_NAME_USE.SidTypeGroup || snu == SID_NAME_USE.SidTypeWellKnownGroup)
                isGroup = true;
            else if (snu == SID_NAME_USE.SidTypeUser)
                isUser = true;

            SecurityDescriptorOwner = new()
            {
                SidTypeGlyph = isGroup ? "\xE902" : (isUser ? "\xE2AF" : "\xE716"),
                Name = outuser.ToString(),
                Domain = outdomain.ToString(),
                Sid = owner.ToString(),
            };

            outuser.Clear();
            outdomain.Clear();

            pSD.Close();
        }

        HKEY RVRegOpenKey(HKEY hkey, string subRoot, REGSAM samDesired, bool use86Arch = false)
        {
            // If specified machine, should use RegConnectRegistry
            var result = RegOpenKeyEx(hkey, subRoot, 0, samDesired, out var phkResult);

            if (result.Succeeded)
                return phkResult;
            else
                return HKEY.NULL;
        }

        unsafe static ref T NullRef<T>()
        {
            return ref Unsafe.AsRef<T>(null);
        }
    }
}
