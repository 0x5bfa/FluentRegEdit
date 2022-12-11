using RegistryValley.App.Models;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RegistryValley.App.ViewModels.Dialogs
{
    public class KeyPermissionsViewerDialogViewModel : ObservableObject
    {
        public KeyPermissionsViewerDialogViewModel()
        {
            _principals = new();
            Principals = new(_principals);

            LoadKeySecurityCommand = new RelayCommand(LoadKeySecurity);
        }

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

        private bool _showSecurityAdvanced;
        public bool ShowAdvancedPermissions { get => _showSecurityAdvanced; set => SetProperty(ref _showSecurityAdvanced, value); }

        private bool _selectedAdvancedPermissionItem;
        public bool SelectedAdvancedPermissionItem { get => _selectedAdvancedPermissionItem; set => SetProperty(ref _selectedAdvancedPermissionItem, value); }

        private PermissionPrincipalItem _selectedPrincipalAdvancedPermission;
        public PermissionPrincipalItem SelectedPrincipalAdvancedPermission { get => _selectedPrincipalAdvancedPermission; set => SetProperty(ref _selectedPrincipalAdvancedPermission, value); }

        public IRelayCommand LoadKeySecurityCommand { get; }

        private void LoadKeySecurity()
        {
            LoadKeySecurityOwner();

            LoadKeySecurityDescriptor();
        }

        public void LoadKeySecurityDescriptor()
        {
            if (_principals != null && _principals.Any())
                _principals?.Clear();

            Win32Error result;
            bool bResult;

            var hkey = RegValleyOpenKey(KeyItem.RootHive, KeyItem.Path, REGSAM.READ_CONTROL);

            var pSD = new SafePSECURITY_DESCRIPTOR(512);
            var sdsz = (uint)pSD.Size;

            result = RegGetKeySecurity(hkey, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSD, ref sdsz);

            bResult = GetSecurityDescriptorDacl(pSD, out var lpbDaclPresent, out var pDacl, out bool lpbDaclDefaulted);
            result = Kernel32.GetLastError();

            if (!lpbDaclPresent)
            {
                HasDacl =  false;
            }
            else
            {
                bResult = GetAclInformation(pDacl, out ACL_SIZE_INFORMATION asi, (uint)Marshal.SizeOf(typeof(ACL_SIZE_INFORMATION)), ACL_INFORMATION_CLASS.AclSizeInformation);
                result = Kernel32.GetLastError();

                HasDacl = true;

                for (var index = 0U; index < asi.AceCount; index++)
                {
                    result = Kernel32.GetLastError();

                    var pAce = pDacl.GetAce(index);
                    result = Kernel32.GetLastError();
                    var sid = pAce.GetSid();

                    var accountSize = 2048;
                    var domainSize = 2048;
                    var outuser = new StringBuilder(accountSize, accountSize);
                    var outdomain = new StringBuilder(domainSize, domainSize);

                    bResult = LookupAccountSid(null, sid, outuser, ref accountSize, outdomain, ref domainSize, out var snu);
                    result = Kernel32.GetLastError();

                    bool isGroup = false;
                    bool isUser = false;

                    if (snu == SID_NAME_USE.SidTypeAlias ||
                        snu == SID_NAME_USE.SidTypeGroup ||
                        snu == SID_NAME_USE.SidTypeWellKnownGroup)
                        isGroup = true;
                    else if (snu == SID_NAME_USE.SidTypeUser)
                        isUser = true;

                    if (result.Failed && result == Win32Error.ERROR_NONE_MAPPED)
                    {
                        // Reset
                        outuser.Clear();
                        outdomain.Clear();
                        isGroup = false;
                        isUser = false;
                    }

                    //var regsam = (REGSAM)pAce.GetMask();
                    var accessMask = new ACCESS_MASK(pAce.GetMask());

                    var principal = new PermissionPrincipalItem()
                    {
                        Glyph = isGroup ? "\xE902" : (isUser ? "\xE2AF" : "\xE716"),
                        Name = outuser.ToString(),
                        Domain = outdomain.ToString(),
                        Sid = pAce.GetSid().ToString(),
                        AccessControlTypeGlyph = pAce.GetHeader().AceType == System.Security.AccessControl.AceType.AccessAllowed ? "\xE73E" : "\xE711",
                        AccessRule = new(),
                    };

                    if (pAce.GetAceType() == System.Security.AccessControl.AceType.AccessAllowed)
                    {
                        principal.AccessRule.GrantsAccessMask = accessMask;
                    }
                    else if (pAce.GetAceType() == System.Security.AccessControl.AceType.AccessDenied)
                    {
                        principal.AccessRule.DeniesAccessMask = accessMask;
                    }

                    _principals.Add(principal);

                    outuser.Clear();
                    outdomain.Clear();
                }
            }

            pSD.Close();
        }

        public void LoadKeySecurityOwner()
        {
            if (KeyItem == null)
                return;

            Win32Error result;
            bool bResult;

            var hkey = RegValleyOpenKey(KeyItem.RootHive, KeyItem.Path, REGSAM.READ_CONTROL);

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

            if (snu == SID_NAME_USE.SidTypeAlias ||
                snu == SID_NAME_USE.SidTypeGroup ||
                snu == SID_NAME_USE.SidTypeWellKnownGroup)
                isGroup = true;
            else if (snu == SID_NAME_USE.SidTypeUser)
                isUser = true;

            SecurityDescriptorOwner = new()
            {
                Glyph = isGroup ? "\xE902" : (isUser ? "\xE2AF" : "\xE716"),
                Name = outuser.ToString(),
                Domain = outdomain.ToString(),
                Sid = owner.ToString(),
            };

            outuser.Clear();
            outdomain.Clear();

            pSD.Close();
        }

        HKEY RegValleyOpenKey(HKEY hkey, string subRoot, REGSAM samDesired, bool use86Arch = false)
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
