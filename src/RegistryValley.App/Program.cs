using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using RegistryValley.App.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static RegistryValley.App.Helpers.UWPToWinAppSDKUpgradeHelpers.InteropHelpers;

namespace RegistryValley.App
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            var proc = System.Diagnostics.Process.GetCurrentProcess();
            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            var activePid = ApplicationData.Current.LocalSettings.Values.Get("INSTANCE_ACTIVE", -1);
            var instance = AppInstance.FindOrRegisterForKey(activePid.ToString());
            if (!instance.IsCurrent)
            {
                RedirectActivationTo(instance, activatedArgs);
                return;
            }

            var currentInstance = AppInstance.FindOrRegisterForKey((-proc.Id).ToString());
            if (currentInstance.IsCurrent)
            {
                currentInstance.Activated += OnActivated;
            }

            //if (!IsAdministrator())
            //{
            //    try
            //    {
            //        using (Process elevatedProcess = new())
            //        {
            //            elevatedProcess.StartInfo.Verb = "RunAs";
            //            elevatedProcess.StartInfo.UseShellExecute = true;
            //            elevatedProcess.StartInfo.FileName = Environment.ProcessPath;
            //            elevatedProcess.Start();
            //        }
            //    }
            //    catch (Win32Exception)
            //    {
            //    }
            //}

            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);

                new App();
            });
        }

        private static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void OnActivated(object? sender, AppActivationArguments args)
        {
            if (App.Current is App thisApp)
            {
                // WINUI3: verify if needed or OnLaunched is called
                thisApp.OnActivated(args);
            }
        }

        private static IntPtr redirectEventHandle = IntPtr.Zero;

        // WinUI3: https://github.com/microsoft/WindowsAppSDK/issues/1709
        public static void RedirectActivationTo(AppInstance keyInstance, AppActivationArguments args)
        {
            redirectEventHandle = CreateEvent(IntPtr.Zero, true, false, null);

            Task.Run(() =>
            {
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
                SetEvent(redirectEventHandle);
            });

            uint CWMO_DEFAULT = 0;
            uint INFINITE = 0xFFFFFFFF;

            _ = CoWaitForMultipleObjects(CWMO_DEFAULT, INFINITE, 1, new IntPtr[] { redirectEventHandle }, out uint handleIndex);
        }
    }
}