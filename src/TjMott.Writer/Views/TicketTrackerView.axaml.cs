using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TjMott.Writer.Views
{
    public partial class TicketTrackerView : UserControl
    {
        public TicketTrackerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
