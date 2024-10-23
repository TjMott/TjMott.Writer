using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Reflection;

namespace TjMott.Writer
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Make sure working directory is set to the EXE's location. This tends to not be the case
            // on Linux when entering CEF install mode using pkexec.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            foreach (var arg in args)
            {
                if (arg.Equals("-installcef", StringComparison.InvariantCultureIgnoreCase))
                {
                    App.InstallCef = true;
                }
            }
            // Initialize CEF.
            if (!App.InstallCef)
            {
                CefNetAppImpl.Initialize();
            }

            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
