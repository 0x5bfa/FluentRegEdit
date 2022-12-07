using static Vanara.PInvoke.AdvApi32;

namespace RegistryValley.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RegConnectRegistry(null, Vanara.PInvoke.HKEY.HKEY_CLASSES_ROOT, out var phkResult);


        }
    }
}