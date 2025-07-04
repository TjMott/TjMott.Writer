using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using TjMott.Writer.Controls;
using TjMott.Writer.ViewModels;
using ReactiveUI;

namespace TjMott.Writer.Views
{
    public partial class NotesView : UserControl
    {
        private QuillJsContainer _noteViewer;
        public NotesView()
        {
            Loaded += NotesView_Loaded;
            InitializeComponent();
        }

        private void NotesView_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Loaded -= NotesView_Loaded;
            // Create in code-behind, for some reason creating this control in
            // XAML causes CefGlue to crash at app startup.
            _noteViewer = new QuillJsContainer();
            _noteViewer.AllowUserEditing = false;
            _noteViewer.EditorLoaded += _noteViewer_EditorLoaded;
            noteViewerContainer.Child = _noteViewer;
        }

        private void _noteViewer_EditorLoaded(object sender, System.EventArgs e)
        {
            _noteViewer.EditorLoaded -= _noteViewer_EditorLoaded;
            _noteViewer.ZoomLevel = AppSettings.Default.editorZoom;

            Binding docBinding = new Binding
            {
                Source = DataContext,
                Path = "SelectedNote.Document"
            };

            _noteViewer.Bind(QuillJsContainer.DocumentProperty, docBinding);
        }
    }
}
