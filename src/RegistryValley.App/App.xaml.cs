using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using Microsoft.Windows.AppLifecycle;
using RegistryValley.App.ViewModels;
using Windows.ApplicationModel;
using Windows.Storage;

namespace RegistryValley.App
{
    public partial class App : Application
    {
        public new static App Current
            => (App)Application.Current;

        public IServiceProvider Services { get; }

        public static string AppVersion =
            $"{Package.Current.Id.Version.Major}." +
            $"{Package.Current.Id.Version.Minor}." +
            $"{Package.Current.Id.Version.Build}." +
            $"{Package.Current.Id.Version.Revision}";

        public App()
        {
            InitializeComponent();

            UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedException;

            Services = ConfigureServices();
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                //.AddSingleton<Utils.ILogger>(new SerilogWrapperLogger(Serilog.Log.Logger))
                //.AddSingleton<ToastService>()
                .AddSingleton<IMessenger>(StrongReferenceMessenger.Default)
                // ViewModels
                .AddSingleton<MainViewModel>()
                .AddSingleton<ValuesViewerViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddTransient<ViewModels.Dialogs.ValueEditingDialogViewModel>()
                .BuildServiceProvider();
        }

        private static void EnsureSettingsAndConfigurationAreBootstrapped()
        {
            //AppSettings ??= new SettingsViewModel();
        }

        private static async Task StartAppCenter()
        {
            //try
            //{
            //    if (!AppCenter.Configured)
            //    {
            //        var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Resources/AppCenterKey.txt"));
            //        var lines = await FileIO.ReadTextAsync(file);
            //        using var document = System.Text.Json.JsonDocument.Parse(lines);
            //        var obj = document.RootElement;
            //        AppCenter.Start(obj.GetProperty("key").GetString(), typeof(Analytics), typeof(Crashes));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.Warn(ex, "AppCenter could not be started.");
            //}
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

            // Initialize MainWindow here
            EnsureWindowIsInitialized();

            EnsureSettingsAndConfigurationAreBootstrapped();

            _ = Window.InitializeApplication(activatedEventArgs.Data);
        }

        public void OnActivated(AppActivationArguments activatedEventArgs)
        {
            Window.DispatcherQueue.EnqueueAsync(async () => await Window.InitializeApplication(activatedEventArgs.Data));
        }

        private void EnsureWindowIsInitialized()
        {
            Window = new MainWindow();
            Window.Activated += Window_Activated;
            //Window.Closed += Window_Closed;
            WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(Window);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.CodeActivated ||
                args.WindowActivationState == WindowActivationState.PointerActivated)
            {
                ApplicationData.Current.LocalSettings.Values["INSTANCE_ACTIVE"] = -System.Diagnostics.Process.GetCurrentProcess().Id;
            }
        }

        void OnNavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
            => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private async void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
            => await AppUnhandledException(e.Exception);

        private async void OnUnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
            => await AppUnhandledException(e.Exception);

        private async Task AppUnhandledException(Exception ex)
        {
            //Services.GetService<Utils.ILogger>()?.Fatal("Unhandled exception", ex);

            try
            {
                await new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Unhandled exception",
                    Content = ex.Message,
                    CloseButtonText = "Close"
                }
                .ShowAsync();
            }
            catch (Exception ex2)
            {
                //Services.GetService<Utils.ILogger>()?.Error("Failed to display unhandled exception", ex2);
            }
        }

        public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
        {
            if (!typeof(TEnum).GetType().IsEnum)
            {
                throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
            }
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }

        public static void CloseApp()
            => Window.Close();

        public static AppWindow GetAppWindow(Window w)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(w);

            Microsoft.UI.WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            return AppWindow.GetFromWindowId(windowId);
        }

        public static MainWindow Window { get; private set; } = null!;

        public static IntPtr WindowHandle { get; private set; }
    }
}
