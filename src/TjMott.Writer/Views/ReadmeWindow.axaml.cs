using Avalonia.Controls;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class ReadmeWindow : Window
    {
        private static ReadmeWindow _instance;

        public static void ShowReadmeWindow()
        {
            if (_instance == null)
                _instance = new ReadmeWindow();
            _instance.Show();
            _instance.Activate();
            _instance.Focus();
        }

        public ReadmeWindow()
        {
            InitializeComponent();
            Closing += ReadmeWindow_Closing;
            OpenWindowsViewModel.Instance.ReadmeWindow = this;
        }

        private void ReadmeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _instance = null;
            OpenWindowsViewModel.Instance.ReadmeWindow = null;
        }

        private void CloseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}
