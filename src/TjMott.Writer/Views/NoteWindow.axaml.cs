using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class NoteWindow : Window
    {
        public NoteWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            if (!Avalonia.Controls.Design.IsDesignMode)
                OpenWindowsViewModel.Instance.NotesWindows.Add(this);
            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void NoteWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (OpenWindowsViewModel.Instance.NotesWindows.Contains(this))
                OpenWindowsViewModel.Instance.NotesWindows.Remove(this);
        }
    }
}
