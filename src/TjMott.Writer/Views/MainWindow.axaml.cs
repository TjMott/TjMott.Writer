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

            Width = AppSettings.Default.mainWindowWidth;
            Height = AppSettings.Default.mainWindowHeight;
        }

        public void Window_Closing(object sender, CancelEventArgs args)
        {
            AppSettings.Default.mainWindowWidth = Width;
            AppSettings.Default.mainWindowHeight = Height;
            AppSettings.Default.Save();
        }
    }
}
