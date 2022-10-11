using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;
using TjMott.Writer.ViewModels;
using TjMott.Writer.ViewModels.CefInstall;
using TjMott.Writer.Views;
using TjMott.Writer.Views.CefInstall;

namespace TjMott.Writer
{
    public partial class App : Application
    {
        public static bool InstallCef = false;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (InstallCef)
                {
                    // Set file cookie used to tell parent process we're up and running,
                    // and we're past the pkexec/UAC prompts.
                    File.WriteAllText(CefNetAppImpl.CefInstallingCookiePath, "true");
                    CefInstallWindow mainWindow = new CefInstallWindow();
                    mainWindow.DataContext = new CefInstallViewModel();
                    desktop.MainWindow = mainWindow;
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();
                    MainWindowViewModel vm = new MainWindowViewModel(mainWindow);
                    mainWindow.DataContext = vm;
                    desktop.MainWindow = mainWindow;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
