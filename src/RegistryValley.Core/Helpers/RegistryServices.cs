using System.Runtime.CompilerServices;
using Vanara.PInvoke;
using static Vanara.PInvoke.AdvApi32;

namespace RegistryValley.Core.Helpers
{
    public static class RegistryServices
    {
        public static HKEY RegValleyOpenKey(HKEY hkey, string subRoot, REGSAM samDesired, bool use86Arch = false)
        {
            // If specified machine, should use RegConnectRegistry
            var result = RegOpenKeyEx(hkey, subRoot, 0, samDesired, out var phkResult);

            if (result.Succeeded)
                return phkResult;
            else
            {
                Kernel32.SetLastError((uint)result);
                return HKEY.NULL;
            }
        }

        public unsafe static ref T NullRef<T>()
        {
            return ref Unsafe.AsRef<T>(null);
        }
    }
}
