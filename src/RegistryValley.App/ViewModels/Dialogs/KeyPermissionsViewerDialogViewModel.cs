using RegistryValley.App.Models;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace RegistryValley.App.ViewModels.Dialogs
{
    public class KeyPermissionsViewerDialogViewModel : ObservableObject
    {
        public KeyPermissionsViewerDialogViewModel()
        {
            _principals = new();
            Principals = new(_principals);

            LoadKeySecurityDescriptorCommand = new RelayCommand(LoadKeySecurityDescriptor);
        }

        private KeyItem _keyItem;
        public KeyItem KeyItem { get => _keyItem; set => SetProperty(ref _keyItem, value); }

        private readonly ObservableCollection<PermissionPrincipalItem> _principals;
        public ReadOnlyObservableCollection<PermissionPrincipalItem> Principals { get; }

        private PermissionPrincipalItem _selectedPrincipal;
        public PermissionPrincipalItem SelectedPrincipal { get => _selectedPrincipal; set => SetProperty(ref _selectedPrincipal, value); }

        private bool _hasDacl;
        public bool HasDacl { get => _hasDacl; set => SetProperty(ref _hasDacl, value); }

        public IRelayCommand LoadKeySecurityDescriptorCommand { get; }

        private void LoadKeySecurityDescriptor()
        {
            _principals.Clear();

            Win32Error result;
            bool bResult;

            var hkey = RegValleyOpenKey(KeyItem.RootHive, KeyItem.Path, REGSAM.READ_CONTROL);

            var pSD = new SafePSECURITY_DESCRIPTOR(256);
            var sdsz = (uint)pSD.Size;

            RegGetKeySecurity(hkey, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSD, ref sdsz);

            GetSecurityDescriptorDacl(pSD, out var lpbDaclPresent, out var pDacl, out bool lpbDaclDefaulted);

            if (!lpbDaclPresent)
            {
                HasDacl =  false;
            }
            else
            {
                bResult = GetAclInformation(pDacl, out ACL_SIZE_INFORMATION asi, (uint)Marshal.SizeOf(typeof(ACL_REVISION_INFORMATION)), ACL_INFORMATION_CLASS.AclSizeInformation);

                HasDacl = true;

                for (var index = 0U; index < pDacl.GetAclInformation<ACL_SIZE_INFORMATION>().AceCount; index++)
                {
                    var pAce = pDacl.GetAce(index);

                    // Should cast pAce to more specifical object, such as ACCESS_ALLOWED_ACE or ACCESS_DENIED_ACE ?

                    var accountSize = 1024;
                    var domainSize = 1024;
                    var outuser = new StringBuilder(accountSize, accountSize);
                    var outdomain = new StringBuilder(domainSize, domainSize);

                    LookupAccountSid(null, pAce.GetSid(), outuser, ref accountSize, outdomain, ref domainSize, out var snu);

                    bool isGroup = false;
                    bool isUser = false;

                    if (snu == SID_NAME_USE.SidTypeAlias ||
                        snu == SID_NAME_USE.SidTypeGroup ||
                        snu == SID_NAME_USE.SidTypeWellKnownGroup)
                        isGroup = true;
                    else if (snu == SID_NAME_USE.SidTypeUser)
                        isUser = true;

                    //var regsam = (REGSAM)pAce.GetMask();
                    var accessMask = new ACCESS_MASK(pAce.GetMask());

                    var principal = new PermissionPrincipalItem()
                    {
                        Glyph = isGroup ? "\xE2AF" : (isUser ? "\xE902" : "\xE716"),
                        Name = outuser.ToString(),
                        Domain = outdomain.ToString(),
                        Sid = pAce.GetSid().ToString(),
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
                }
            }

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
