using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;
using TjMott.Writer.ViewModels;
using TjMott.Writer.Views;

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
                MainWindow mainWindow = new MainWindow();
                MainWindowViewModel vm = new MainWindowViewModel(mainWindow);
                mainWindow.DataContext = vm;
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
