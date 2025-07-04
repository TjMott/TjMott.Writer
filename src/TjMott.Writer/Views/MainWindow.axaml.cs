using Avalonia.Controls;
using System.ComponentModel;
using TjMott.Writer.ViewModels;
using System.Linq;

namespace TjMott.Writer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += Window_Closing;

            Width = AppSettings.Default.mainWindowWidth;
            Height = AppSettings.Default.mainWindowHeight;

            Title += " v" + GetType().Assembly.GetName().Version.ToString();
        }

        public async void Window_Closing(object sender, CancelEventArgs args)
        {
            MainWindowViewModel vm = (MainWindowViewModel)DataContext;

            if (vm.OpenWindowsViewModel.AllWindows.Count() > 1)
            {
                args.Cancel = true;
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Open Windows",
                    "You have several open windows. Please save and close your work before closing the main window.",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
                return;
            }


            AppSettings.Default.mainWindowWidth = Width;
            AppSettings.Default.mainWindowHeight = Height;
            AppSettings.Default.Save();
        }
    }
}
