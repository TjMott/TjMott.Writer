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
            var wnd = this.Parent;
            while (!(wnd is Window))
                wnd = wnd.Parent;
            CefNetAppImpl.RestartAndInstallCef((Window)wnd);
        }
    }
}
