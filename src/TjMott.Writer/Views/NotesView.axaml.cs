using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TjMott.Writer.Views
{
    public partial class NotesView : UserControl
    {
        public NotesView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
