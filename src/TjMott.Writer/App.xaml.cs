using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using TjMott.Writer.Properties;
using TjMott.Writer.Windows;

namespace TjMott.Writer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string StartupFileName { get; private set; }
        public static OSPlatform OperatingSystem { get; private set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                OperatingSystem = OSPlatform.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                OperatingSystem = OSPlatform.Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                OperatingSystem = OSPlatform.OSX;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                OperatingSystem = OSPlatform.FreeBSD;
            
            if (OperatingSystem != OSPlatform.Windows)
            {
                Console.WriteLine("Unsupported operating system {0}. The application will now exit.", OperatingSystem);
                Shutdown(0);
            }

            if (e.Args.Length > 0)
            {
                StartupFileName = e.Args[0];
            }
            else if (!string.IsNullOrWhiteSpace(AppSettings.Default.lastFile) && File.Exists(AppSettings.Default.lastFile))
            {
                StartupFileName = AppSettings.Default.lastFile;
            }
            MainWindow wnd = new MainWindow();
            wnd.Show();
            //DbConverter.Convert();
            //Shutdown(0);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

        }
    }
}
