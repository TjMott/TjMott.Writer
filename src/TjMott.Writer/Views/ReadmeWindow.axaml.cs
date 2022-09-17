using Avalonia.Controls;

namespace TjMott.Writer.Views
{
    public partial class ReadmeWindow : Window
    {
        public ReadmeWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}
