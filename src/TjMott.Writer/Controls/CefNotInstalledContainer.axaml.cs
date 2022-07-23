using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TjMott.Writer.Controls
{
    public partial class CefNotInstalledContainer : UserControl
    {
        public CefNotInstalledContainer()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InstallButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            CefNetAppImpl.RestartAndInstallCef();
        }
    }
}
