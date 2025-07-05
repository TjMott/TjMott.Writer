using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;

namespace TjMott.Writer
{
    internal class Program
    {
        /// <summary>
        /// Will contain a filename to a works database if the user launched this application
        /// by double-clicking a database in Windows Explorer. Null otherwise.
        /// </summary>
        public static string DoubleClickedDatabaseFile { get; private set; }

        private static string _rootCachePath;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Set working directory to the executable location. Some things (such as double-clicking
            // a .wdb file to launch the app) set the working directory to someplace that causes a crash.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); 

            // Code to assist with remote debugging for Linux.
            if (args.Contains("--wait-for-attach"))
            {
                Console.WriteLine("Waiting for debugger to attach...");
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                    if (Debugger.IsAttached)
                    {
                        Console.WriteLine("Debugger attached.");
                    }
                }
            }

            // Check for .wdb file in args, in case user double-clicked a .wdb file.
            foreach (var arg in args)
            {
                if (arg.ToLower().EndsWith(".wdb"))
                {
                    DoubleClickedDatabaseFile = arg;
                }
            }

            AppDomain.CurrentDomain.ProcessExit += delegate { Cleanup(); };

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .AfterSetup(_ =>
                {
                    // Initialize theme from persisted user settings.
                    if (AppSettings.Default.selectedTheme == "light")
                        Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                    else if (AppSettings.Default.selectedTheme == "dark")
                        Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                    else
                        Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;

                    // generate a unique cache path to avoid problems when launching more than one process
                    // https://www.magpcss.org/ceforum/viewtopic.php?f=6&t=19665
                    _rootCachePath = Path.Combine(Path.GetTempPath(), "CefGlue_" + Guid.NewGuid().ToString().Replace("-", null));
                    CefRuntimeLoader.Initialize(new CefSettings()
                    {
                        // Make sure cache paths are in a temp folder. If not specified,
                        // CefGlue will use the application directory, which is not writeable.
                        RootCachePath = _rootCachePath,
                        CachePath = Path.Combine(_rootCachePath, "cache"),
                        LogFile = Path.Combine(_rootCachePath, "debug.log"),
                        LocalesDirPath = Path.Combine(_rootCachePath, "locales"),
                        PersistSessionCookies = false,
                        PersistUserPreferences = false,
                        WindowlessRenderingEnabled = false
                    });
                })
                .LogToTrace()
                .UseReactiveUI();

        private static void Cleanup()
        {
            CefRuntime.Shutdown(); // must shutdown cef to free cache files (so that cleanup is able to delete files)

            try
            {
                var dirInfo = new DirectoryInfo(_rootCachePath);
                if (dirInfo.Exists)
                {
                    dirInfo.Delete(true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // ignore
            }
            catch (IOException)
            {
                // ignore
            }
        }
    }
}
