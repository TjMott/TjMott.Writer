using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TjMott.Writer.Controls;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class SceneEditorWindow : Window
    {
        #region Instancing stuff
        private static Dictionary<long, SceneEditorWindow> _windows  = new Dictionary<long, SceneEditorWindow>();
        public static void ShowEditorWindow(SceneViewModel scene)
        {
            try
            {
                if (!_windows.ContainsKey(scene.Model.id))
                {
                    SceneEditorWindow wnd = new SceneEditorWindow(scene);
                    _windows[scene.Model.id] = wnd;
                }

                _windows[scene.Model.id].Show();
                _windows[scene.Model.id].Activate();
                _windows[scene.Model.id].Focus();
            }
            catch (CryptographicException)
            {

            }
            catch (ApplicationException)
            {

            }
        }
        #endregion

        private Document _sceneManuscript;
        private QuillJsContainer _manuscriptEditor;

        #region Unused constructor -- still needed to compile
        public SceneEditorWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        #endregion

        public SceneEditorWindow(SceneViewModel scene)
        {
            Scene = scene;
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
                Title = "Editing Scene: " + Scene.Model.Name;

                _manuscriptEditor = this.FindControl<QuillJsContainer>("manuscriptEditor");
                _manuscriptEditor.EditorLoaded += _manuscriptEditor_EditorLoaded;
                this.Width = AppSettings.Default.editorWindowWidth;
                this.Height = AppSettings.Default.editorWindowHeight;
                Closing += SceneEditorWindow_Closing;
                _sceneManuscript = new Document(Scene.Model.Connection);
                _sceneManuscript.id = Scene.Model.DocumentId;
                AddHandler(KeyDownEvent, onKeyDown);
                OpenWindowsViewModel.Instance.EditorWindows.Add(this);
            }
        }

        private async void _manuscriptEditor_EditorLoaded(object sender, EventArgs e)
        {
            _manuscriptEditor.EditorLoaded -= _manuscriptEditor_EditorLoaded;
            _manuscriptEditor.ZoomLevel = AppSettings.Default.editorZoom;
            await _sceneManuscript.LoadAsync();
            updateMenuItems();
            _manuscriptEditor.Document = _sceneManuscript;
            _manuscriptEditor.Document.PropertyChanged += Document_PropertyChanged;
        }

        private void Document_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateMenuItems();
        }

        private async void SceneEditorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (await _manuscriptEditor.HasUnsavedEdits())
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
                    await _manuscriptEditor.Save();
                    setStatusText("Document saved.", 0);
                }
                else if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.Cancel)
                {
                    setStatusText("Close cancelled.", 5000);
                    return;
                }
            }

            if (_sceneManuscript.IsUnlocked && _sceneManuscript.IsEncrypted)
            {
                _sceneManuscript.Lock();
            }
            Closing -= SceneEditorWindow_Closing;
            if (_windows.ContainsKey(Scene.Model.id))
                _windows.Remove(Scene.Model.id);
            AppSettings.Default.editorWindowWidth = this.Width;
            AppSettings.Default.editorWindowHeight = this.Height;
            AppSettings.Default.editorZoom = _manuscriptEditor.ZoomLevel;
            AppSettings.Default.Save();

            if (OpenWindowsViewModel.Instance.EditorWindows.Contains(this))
                OpenWindowsViewModel.Instance.EditorWindows.Remove(this);

            Close();
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sceneManuscript.IsUnlocked)
            {
                _manuscriptEditor.Print(string.Format("Scene: {0}", Scene.Model.Name));
            }
        }

        public SceneViewModel Scene { get; private set; }

        private void updateMenuItems()
        {
            this.FindControl<MenuItem>("saveButton").IsEnabled = _sceneManuscript.IsUnlocked;
            this.FindControl<MenuItem>("revertButton").IsEnabled = _sceneManuscript.IsUnlocked;
            this.FindControl<MenuItem>("exportButton").IsEnabled = _sceneManuscript.IsUnlocked;
            this.FindControl<MenuItem>("lockButton").IsEnabled = _sceneManuscript.IsEncrypted && _sceneManuscript.IsUnlocked;
            this.FindControl<MenuItem>("encryptButton").IsEnabled = !_sceneManuscript.IsEncrypted;
            this.FindControl<MenuItem>("decryptButton").IsEnabled = _sceneManuscript.IsEncrypted;
            this.FindControl<MenuItem>("printButton").IsEnabled = _sceneManuscript.IsUnlocked;
        }

        private async void onKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.S && e.KeyModifiers == Avalonia.Input.KeyModifiers.Control)
            {
                setStatusText("Saving document...", 0);
                await _manuscriptEditor.Save();
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
