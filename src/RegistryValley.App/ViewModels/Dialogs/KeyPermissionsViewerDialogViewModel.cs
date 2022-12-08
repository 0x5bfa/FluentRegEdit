using RegistryValley.App.Models;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
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
            uint cbSecurityDescriptor = 0;
            Win32Error result;
            bool bResult;

            StringBuilder lpName = new(256);
            int cchAccountNameSize = 0;
            StringBuilder lpReferencedDomain = new(256);
            int cchDomainNameSize = 0;

            result = RegGetKeySecurity(
                KeyItem.RootHive,
                SECURITY_INFORMATION.SACL_SECURITY_INFORMATION,
                IntPtr.Zero,
                ref cbSecurityDescriptor
                );

            IntPtr pSecurityDescriptor = Marshal.AllocHGlobal((int)cbSecurityDescriptor);

            result = RegGetKeySecurity(
                KeyItem.RootHive,
                SECURITY_INFORMATION.SACL_SECURITY_INFORMATION,
                pSecurityDescriptor,
            ref cbSecurityDescriptor
            );

            bResult = GetSecurityDescriptorDacl(
                pSecurityDescriptor,
                out var lpbDaclPresent,
                out var pDacl,
                out var lpbDaclDefaulted
                );

            if (!lpbDaclPresent)
            {
                // No DACL
            }
            else
            {
                uint cAclInformation = 0;

                unsafe
                {
                    cAclInformation = (uint)sizeof(ACL_SIZE_INFORMATION);

                    IntPtr pAclInformation = (IntPtr)GCHandle.Alloc((int)cAclInformation);

                    bResult = GetAclInformation(pDacl, pAclInformation, cAclInformation, ACL_INFORMATION_CLASS.AclSizeInformation);

                    for (uint index = 0; index < ((ACL_SIZE_INFORMATION)GCHandle.FromIntPtr(pAclInformation).Target).AceCount; index++)
                    {
                        bResult = GetAce(pDacl, index, out var pAce);

                        cchAccountNameSize = lpName.Length;
                        cchDomainNameSize = lpReferencedDomain.Length;

                        LookupAccountSid(
                            null,
                            pAce.GetSid().GetBytes(),
                            lpName,
                            ref cchAccountNameSize,
                            lpReferencedDomain,
                            ref cchDomainNameSize,
                            out var peUse
                            );
                    }
                }
            }
        }
    }
}
