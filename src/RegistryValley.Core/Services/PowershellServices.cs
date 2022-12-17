using System.ComponentModel;
using System.Diagnostics;

namespace RegistryValley.Core.Services
{
    public static class PowershellServices
    {
        public static bool RunPowershellCommand(bool runAs, string command)
        {
            try
            {
                using Process process = new();
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.Arguments = command;

                if (runAs)
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.Verb = "runas";
                }

                process.Start();

                if (process.WaitForExit(30 * 1000))
                    return process.ExitCode == 0;

                return false;
            }
            catch (Win32Exception)
            {
                // If user cancels UAC
                return false;
            }
        }
    }
}
