using RegistryValley.App.Models;
using System;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Vanara.PInvoke;
using static Vanara.PInvoke.AdvApi32;

namespace RegistryValley.App.ViewModels.Dialogs
{
    public class KeyPermissionsViewerDialogViewModel : ObservableObject
    {
        public KeyPermissionsViewerDialogViewModel()
        {
            LoadKeySecurityDescriptorCommand = new RelayCommand(LoadKeySecurityDescriptor);
        }

        private KeyItem _keyItem;
        public KeyItem KeyItem { get => _keyItem; set => SetProperty(ref _keyItem, value); }

        public IRelayCommand LoadKeySecurityDescriptorCommand { get; }

        private void LoadKeySecurityDescriptor()
        {
            Win32Error result;
            bool bResult;

            var hkey = RegValleyOpenKey(
                KeyItem.RootHive,
                KeyItem.Path,
                REGSAM.READ_CONTROL
                );

            var pSD = new SafePSECURITY_DESCRIPTOR(256);
            var sdsz = (uint)pSD.Size;

            RegGetKeySecurity(hkey, SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, pSD, ref sdsz);

            GetSecurityDescriptorDacl(pSD, out var lpbDaclPresent, out var pDacl, out bool lpbDaclDefaulted);

            if (!lpbDaclPresent)
            {
                // No DACL
            }
            else
            {
                bResult = GetAclInformation(pDacl, out ACL_SIZE_INFORMATION asi, (uint)Marshal.SizeOf(typeof(ACL_REVISION_INFORMATION)), ACL_INFORMATION_CLASS.AclSizeInformation);

                for (var i = 0U; i < asi.AceCount; i++)
                {
                    GetAce(pDacl, i, out var pAce);

                    var accountSize = 1024;
                    var domainSize = 1024;
                    var outuser = new StringBuilder(accountSize, accountSize);
                    var outdomain = new StringBuilder(domainSize, domainSize);

                    LookupAccountSid(null, pAce.GetSid(), outuser, ref accountSize, outdomain, ref domainSize, out _);
                    System.WriteLine($"Ace{i}: {pAce.GetHeader().AceType}={outdomain}\\{outuser}; {pAce.GetMask()}");
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
