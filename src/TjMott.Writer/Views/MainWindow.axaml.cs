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

            Width = AppSettings.Default.mainWindowWidth;
            Height = AppSettings.Default.mainWindowHeight;
        }

        public async void Window_Closing(object sender, CancelEventArgs args)
        {
            MainWindowViewModel vm = (MainWindowViewModel)DataContext;

            if (vm.OpenWindowsViewModel.AllWindows.Count() > 1)
            {
                args.Cancel = true;
                await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Open Windows",
                    "You have several open windows. Please save and close your work before closing the main window.",
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation.CenterOwner).ShowDialog(this);
                return;
            }


            AppSettings.Default.mainWindowWidth = Width;
            AppSettings.Default.mainWindowHeight = Height;
            AppSettings.Default.Save();
        }
    }
}
