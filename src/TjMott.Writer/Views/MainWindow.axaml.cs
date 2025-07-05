using Avalonia.Controls;
using System.ComponentModel;
using TjMott.Writer.ViewModels;

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

        public void UnsubscribeClosingEvent()
        {
            Closing -= Window_Closing;
        }

        public async void Window_Closing(object sender, CancelEventArgs args)
        {
            args.Cancel = true;
            
            MainWindowViewModel vm = (MainWindowViewModel)DataContext;

            // VM will deal with open windows, committing the database, etc.
            await vm.QuitAsync(this);
        }
    }
}
