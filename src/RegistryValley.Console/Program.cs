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