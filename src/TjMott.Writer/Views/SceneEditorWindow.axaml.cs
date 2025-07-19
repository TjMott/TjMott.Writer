using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
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

        #region Unused constructor -- still needed to compile
        public SceneEditorWindow()
        {
            InitializeComponent();
            internalInitialize();
        }

        #endregion

        public SceneEditorWindow(SceneViewModel scene)
        {
            Scene = scene;
            InitializeComponent();
            internalInitialize();
        }

        private void internalInitialize()
        {
            if (!Avalonia.Controls.Design.IsDesignMode)
            {
                Title = "Editing Scene: " + Scene.Model.Name;

                manuscriptEditor.EditorLoaded += _manuscriptEditor_EditorLoaded;
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
            manuscriptEditor.EditorLoaded -= _manuscriptEditor_EditorLoaded;
            manuscriptEditor.ZoomLevel = AppSettings.Default.editorZoom;
            await _sceneManuscript.LoadAsync();
            updateMenuItems();
            manuscriptEditor.Document = _sceneManuscript;
            manuscriptEditor.Document.PropertyChanged += Document_PropertyChanged;
        }

        private void Document_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateMenuItems();
        }

        private async void SceneEditorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (manuscriptEditor.HasUnsavedEdits())
            {
                var msgBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Save Before Closing?",
                    "Your document has unsaved edits. Save before closing?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                    MsBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation.CenterOwner);
                var msgBoxResult = await msgBox.ShowWindowDialogAsync(this);
                if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    setStatusText("Saving document...", 0);
                    await manuscriptEditor.Save();
                    setStatusText("Document saved.", 0);
                }
                else if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Cancel)
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
            AppSettings.Default.editorZoom = manuscriptEditor.ZoomLevel;
            AppSettings.Default.Save();

            if (OpenWindowsViewModel.Instance.EditorWindows.Contains(this))
                OpenWindowsViewModel.Instance.EditorWindows.Remove(this);

            manuscriptEditorBorder.Child = null;
            manuscriptEditor.Dispose();

            Close();
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sceneManuscript.IsUnlocked)
            {
                manuscriptEditor.Print(string.Format("Scene: {0}", Scene.Model.Name));
            }
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportToWordWindow.ShowExportWindow(Scene);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public SceneViewModel Scene { get; private set; }

        private void updateMenuItems()
        {
            saveButton.IsEnabled = _sceneManuscript.IsUnlocked;
            revertButton.IsEnabled = _sceneManuscript.IsUnlocked;
            exportButton.IsEnabled = !_sceneManuscript.IsEncrypted;
            lockButton.IsEnabled = _sceneManuscript.IsEncrypted && _sceneManuscript.IsUnlocked;
            encryptButton.IsEnabled = !_sceneManuscript.IsEncrypted;
            decryptButton.IsEnabled = _sceneManuscript.IsEncrypted;
            
            // I don't know how to print with CefGlue, so leave disabled for now.
            printButton.IsEnabled = _sceneManuscript.IsUnlocked;
        }

        private async void onKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.S && e.KeyModifiers == Avalonia.Input.KeyModifiers.Control)
            {
                setStatusText("Saving document...", 0);
                await manuscriptEditor.Save();
                setStatusText("Document saved.", 5000);
            }
        }

        private async void setStatusText(string text, int timeoutMillis)
        {
            statusBarTextBlock.Text = text;
            if (timeoutMillis > 0)
            {
                await Task.Delay(timeoutMillis);
                statusBarTextBlock.Text = "";
            }
        }
    }
}
