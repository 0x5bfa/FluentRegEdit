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

            LoadKeySecurityDescriptorCommand = new RelayCommand(LoadKeySecurityDescriptor);
        }

        private KeyItem _keyItem;
        public KeyItem KeyItem { get => _keyItem; set => SetProperty(ref _keyItem, value); }

        private readonly ObservableCollection<PermissionPrincipalItem> _principals;
        public ReadOnlyObservableCollection<PermissionPrincipalItem> Principals { get; }

        private PermissionPrincipalItem _selectedPrincipal;
        public PermissionPrincipalItem SelectedPrincipal { get => _selectedPrincipal; set => SetProperty(ref _selectedPrincipal, value); }

        public IRelayCommand LoadKeySecurityDescriptorCommand { get; }

        private void LoadKeySecurityDescriptor()
        {
            _principals.Clear();

            Win32Error result;
            bool bResult;

            var hkey = RegValleyOpenKey(
                KeyItem.RootHive,
                KeyItem.Path,
                REGSAM.READ_CONTROL
                );

            var pSD = new SafePSECURITY_DESCRIPTOR(256);
            var sdsz = (uint)pSD.Size;

            RegGetKeySecurity(hkey, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, pSD, ref sdsz);

            GetSecurityDescriptorDacl(pSD, out var lpbDaclPresent, out var pDacl, out bool lpbDaclDefaulted);

            if (!lpbDaclPresent)
            {
                // No DACL
            }
            else
            {
                bResult = GetAclInformation(pDacl, out ACL_SIZE_INFORMATION asi, (uint)Marshal.SizeOf(typeof(ACL_REVISION_INFORMATION)), ACL_INFORMATION_CLASS.AclSizeInformation);

                for (var index = 0U; index < pDacl.GetAclInformation<ACL_SIZE_INFORMATION>().AceCount; index++)
                {
                    var pAce = pDacl.GetAce(index);

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

                    _principals.Add(new()
                    {
                        Glyph = isGroup ? "\xE2AF" : (isUser ? "\xE902" : "\xE716"),
                        Name = outuser.ToString(),
                        Domain = outdomain.ToString(),
                        Sid = pAce.GetSid().ToString(),
                    });

                    //System.Console.WriteLine($"Ace: {index}: Type: {pAce.GetHeader().AceType} Account: {outdomain}\\{outuser} Mask: {pAce.GetMask()}");

                    //if ((pAce.GetMask() & ACCESS_MASK.DELETE) == ACCESS_MASK.DELETE)
                    //    System.Console.WriteLine("DELETE");
                    //if ((pAce.GetMask() & ACCESS_MASK.READ_CONTROL) == ACCESS_MASK.READ_CONTROL)
                    //    System.Console.WriteLine("READ_CONTROL");
                    //if ((pAce.GetMask() & ACCESS_MASK.WRITE_DAC) == ACCESS_MASK.WRITE_DAC)
                    //    System.Console.WriteLine("WRITE_DAC");
                    //if ((pAce.GetMask() & ACCESS_MASK.WRITE_OWNER) == ACCESS_MASK.WRITE_OWNER)
                    //    System.Console.WriteLine("WRITE_OWNER");
                    //if ((pAce.GetMask() & ACCESS_MASK.SYNCHRONIZE) == ACCESS_MASK.SYNCHRONIZE)
                    //    System.Console.WriteLine("SYNCHRONIZE");
                    //if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_REQUIRED) == ACCESS_MASK.STANDARD_RIGHTS_REQUIRED)
                    //    System.Console.WriteLine("STANDARD_RIGHTS_REQUIRED");
                    //if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_READ) == ACCESS_MASK.STANDARD_RIGHTS_READ)
                    //    System.Console.WriteLine("STANDARD_RIGHTS_READ");
                    //if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_WRITE) == ACCESS_MASK.STANDARD_RIGHTS_WRITE)
                    //    System.Console.WriteLine("STANDARD_RIGHTS_WRITE");
                    //if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_EXECUTE) == ACCESS_MASK.STANDARD_RIGHTS_EXECUTE)
                    //    System.Console.WriteLine("STANDARD_RIGHTS_EXECUTE");
                    //if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_ALL) == ACCESS_MASK.STANDARD_RIGHTS_ALL)
                    //    System.Console.WriteLine("STANDARD_RIGHTS_ALL");
                    //if ((pAce.GetMask() & ACCESS_MASK.SPECIFIC_RIGHTS_ALL) == ACCESS_MASK.SPECIFIC_RIGHTS_ALL)
                    //    System.Console.WriteLine("SPECIFIC_RIGHTS_ALL");
                }
            }
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
