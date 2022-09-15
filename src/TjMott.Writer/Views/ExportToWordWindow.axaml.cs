using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class ExportToWordWindow : Window
    {
        public ExportToWordWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Activated += ExportToWordWindow_Activated;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void ExportToWordWindow_Activated(object sender, System.EventArgs e)
        {
            Activated -= ExportToWordWindow_Activated;
            await ((ExportToWordViewModel)DataContext).ExportAsync(this);
        }
    }
}
