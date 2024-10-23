using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TjMott.Writer.Controls;

namespace TjMott.Writer.Views
{
    public partial class NotesView : UserControl
    {
        public NotesView()
        {
            InitializeComponent();
        }

        /*private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<QuillJsContainer>("notePreviewContainer").ZoomLevel = AppSettings.Default.editorZoom;
        }*/
    }
}
