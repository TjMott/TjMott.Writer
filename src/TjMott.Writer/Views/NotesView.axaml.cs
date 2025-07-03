using Avalonia;
using Avalonia.Controls;

namespace TjMott.Writer.Views
{
    public partial class NotesView : UserControl
    {
        public NotesView()
        {
            InitializeComponent();
            notePreviewContainer.ZoomLevel = AppSettings.Default.editorZoom;
        }
    }
}
