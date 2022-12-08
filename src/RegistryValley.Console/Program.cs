using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using Vanara.PInvoke;
using static Vanara.PInvoke.AdvApi32;
using static Vanara.PInvoke.Ole32;

namespace RegistryValley.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            Win32Error result;
            bool bResult;

            var hkey = RegValleyOpenKey(
                HKEY.HKEY_LOCAL_MACHINE,
                "SOFTWARE",
                REGSAM.KEY_ALL_ACCESS
                );

            var pSD = new SafePSECURITY_DESCRIPTOR(1024);
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

                    LookupAccountSid(null, pAce.GetSid(), outuser, ref accountSize, outdomain, ref domainSize, out _);
                    System.Console.WriteLine($"Ace: {index}: Type: {pAce.GetHeader().AceType} Account: {outdomain}\\{outuser} Mask: {pAce.GetMask()}");

                    if ((pAce.GetMask() & ACCESS_MASK.DELETE) == ACCESS_MASK.DELETE)
                        System.Console.WriteLine("DELETE");
                    if ((pAce.GetMask() & ACCESS_MASK.READ_CONTROL) == ACCESS_MASK.READ_CONTROL)
                        System.Console.WriteLine("READ_CONTROL");
                    if ((pAce.GetMask() & ACCESS_MASK.WRITE_DAC) == ACCESS_MASK.WRITE_DAC)
                        System.Console.WriteLine("WRITE_DAC");
                    if ((pAce.GetMask() & ACCESS_MASK.WRITE_OWNER) == ACCESS_MASK.WRITE_OWNER)
                        System.Console.WriteLine("WRITE_OWNER");
                    if ((pAce.GetMask() & ACCESS_MASK.SYNCHRONIZE) == ACCESS_MASK.SYNCHRONIZE)
                        System.Console.WriteLine("SYNCHRONIZE");
                    if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_REQUIRED) == ACCESS_MASK.STANDARD_RIGHTS_REQUIRED)
                        System.Console.WriteLine("STANDARD_RIGHTS_REQUIRED");
                    if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_READ) == ACCESS_MASK.STANDARD_RIGHTS_READ)
                        System.Console.WriteLine("STANDARD_RIGHTS_READ");
                    if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_WRITE) == ACCESS_MASK.STANDARD_RIGHTS_WRITE)
                        System.Console.WriteLine("STANDARD_RIGHTS_WRITE");
                    if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_EXECUTE) == ACCESS_MASK.STANDARD_RIGHTS_EXECUTE)
                        System.Console.WriteLine("STANDARD_RIGHTS_EXECUTE");
                    if ((pAce.GetMask() & ACCESS_MASK.STANDARD_RIGHTS_ALL) == ACCESS_MASK.STANDARD_RIGHTS_ALL)
                        System.Console.WriteLine("STANDARD_RIGHTS_ALL");
                    if ((pAce.GetMask() & ACCESS_MASK.SPECIFIC_RIGHTS_ALL) == ACCESS_MASK.SPECIFIC_RIGHTS_ALL)
                        System.Console.WriteLine("SPECIFIC_RIGHTS_ALL");
                }
            }
        }

        private static HKEY RegValleyOpenKey(HKEY hkey, string subRoot, REGSAM samDesired, bool use86Arch = false)
        {
            // If specified machine, should use RegConnectRegistry
            var result = RegOpenKeyEx(hkey, subRoot, RegOpenOptions.REG_OPTION_NON_VOLATILE, samDesired, out var phkResult);

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