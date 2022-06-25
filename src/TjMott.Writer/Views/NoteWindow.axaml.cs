using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TjMott.Writer.Controls;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class NoteWindow : Window
    {
        #region Instancing stuff
        private static Dictionary<long, NoteWindow> _windows = new Dictionary<long, NoteWindow>();
        public static void ShowEditorWindow(NoteDocumentViewModel vm)
        {
            if (!_windows.ContainsKey(vm.Model.id))
            {
                NoteWindow wnd = new NoteWindow(vm);
                _windows[vm.Model.id] = wnd;
            }

            _windows[vm.Model.id].Show();
            _windows[vm.Model.id].Activate();
            _windows[vm.Model.id].Focus();
        }
        #endregion


        private NoteDocumentViewModel _noteDocViewModel;
        private Document _document;
        private QuillJsContainer _documentEditor;

        public NoteWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public NoteWindow(NoteDocumentViewModel vm)
        {
            DataContext = vm;
            _noteDocViewModel = vm;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            if (!Avalonia.Controls.Design.IsDesignMode)
            {
                Title = "Editing Scene: " + _noteDocViewModel.Model.Name;

                _documentEditor = this.FindControl<QuillJsContainer>("noteEditor");
                _documentEditor.EditorLoaded += _documentEditor_EditorLoaded;
                this.Width = AppSettings.Default.editorWindowWidth;
                this.Height = AppSettings.Default.editorWindowHeight;
                Closing += NoteWindow_Closing;
                _document = new Document(_noteDocViewModel.Model.Connection);
                _document.id = _noteDocViewModel.Model.DocumentId;
                AddHandler(KeyDownEvent, onKeyDown);
                OpenWindowsViewModel.Instance.NotesWindows.Add(this);
            }
        }

        private async void _documentEditor_EditorLoaded(object sender, EventArgs e)
        {
            _documentEditor.EditorLoaded -= _documentEditor_EditorLoaded;
            _documentEditor.ZoomLevel = AppSettings.Default.editorZoom;
            await _document.LoadAsync();

            _documentEditor.Document = _document;
        }

        private async void NoteWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (await _documentEditor.HasUnsavedEdits())
            {
                var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Save Before Closing?",
                    "Your document has unsaved edits. Save before closing?",
                    MessageBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                    MessageBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation.CenterOwner);
                var msgBoxResult = await msgBox.ShowDialog(this);
                if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    setStatusText("Saving document...", 0);
                    await _documentEditor.Save();
                    setStatusText("Document saved.", 0);
                }
                else if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.Cancel)
                {
                    setStatusText("Close cancelled.", 5000);
                    return;
                }
            }

            Closing -= NoteWindow_Closing;
            if (_windows.ContainsKey(_noteDocViewModel.Model.id))
                _windows.Remove(_noteDocViewModel.Model.id);
            AppSettings.Default.editorWindowWidth = this.Width;
            AppSettings.Default.editorWindowHeight = this.Height;
            AppSettings.Default.editorZoom = _documentEditor.ZoomLevel;
            AppSettings.Default.Save();

            if (OpenWindowsViewModel.Instance.NotesWindows.Contains(this))
                OpenWindowsViewModel.Instance.NotesWindows.Remove(this);

            Close();
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            if (_document.IsUnlocked)
            {
                _documentEditor.Print(string.Format("Note: {0}", _noteDocViewModel.Model.Name));
            }
        }

        private async void onKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.S && e.KeyModifiers == Avalonia.Input.KeyModifiers.Control)
            {
                setStatusText("Saving document...", 0);
                await _documentEditor.Save();
                setStatusText("Document saved.", 5000);
            }
        }

        private async void setStatusText(string text, int timeoutMillis)
        {
            this.FindControl<TextBlock>("statusBarTextBlock").Text = text;
            if (timeoutMillis > 0)
            {
                await Task.Delay(timeoutMillis);
                this.FindControl<TextBlock>("statusBarTextBlock").Text = "";
            }
        }
    }
}
