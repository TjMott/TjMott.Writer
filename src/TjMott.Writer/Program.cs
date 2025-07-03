using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Reflection;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;

namespace TjMott.Writer
{
    internal class Program
    {

        private static string _cachePath;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += delegate { Cleanup(); };

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions())
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
                    _cachePath = Path.Combine(Path.GetTempPath(), "CefGlue_" + Guid.NewGuid().ToString().Replace("-", null));
                    CefRuntimeLoader.Initialize(new CefSettings()
                    {
                        RootCachePath = _cachePath,
#if WINDOWLESS
                            // its recommended to leave this off (false), since its less performant and can cause more issues
                            WindowlessRenderingEnabled = true
#else
                        WindowlessRenderingEnabled = false
#endif
                    });
                })
                .LogToTrace()
                .UseReactiveUI();

        private static void Cleanup()
        {
            CefRuntime.Shutdown(); // must shutdown cef to free cache files (so that cleanup is able to delete files)

            try
            {
                var dirInfo = new DirectoryInfo(_cachePath);
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
