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
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
