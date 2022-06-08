using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
        private TextBlock _wordCountTextBlock;

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
            
            _sceneManuscript = new Document(Scene.Model.Connection);
            _sceneManuscript.id = Scene.Model.DocumentId;
            _sceneManuscript.LoadAsync().Wait();
        }

        private void ZoomSlider_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && _manuscriptEditor != null && e.Property.Name == "Value")
            {
                _manuscriptEditor.ZoomLevel = (double)e.NewValue;
            }
        }

        private void _manuscriptEditor_EditorLoaded(object sender, EventArgs e)
        {
            _manuscriptEditor.EditorLoaded -= _manuscriptEditor_EditorLoaded;
            _manuscriptEditor.Document = _sceneManuscript;
            this.FindControl<Slider>("zoomSlider").Value = AppSettings.Default.editorZoom;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Title = "Editing Scene: " + Scene.Model.Name;

            _wordCountTextBlock = this.FindControl<TextBlock>("wordCountTextBlock");
            _manuscriptEditor = this.FindControl<QuillJsContainer>("manuscriptEditor");
            _manuscriptEditor.EditorLoaded += _manuscriptEditor_EditorLoaded;
            _manuscriptEditor.TextChanged += _manuscriptEditor_TextChanged;
            this.Width = AppSettings.Default.editorWindowWidth;
            this.Height = AppSettings.Default.editorWindowHeight;
            Closing += SceneEditorWindow_Closing;
        }

        private async void _manuscriptEditor_TextChanged(object sender, EventArgs e)
        {
            int wordCount = await _manuscriptEditor.GetWordCount();
            _wordCountTextBlock.Text = string.Format("Word Count: {0}", wordCount);
        }

        private void SceneEditorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= SceneEditorWindow_Closing;
            if (_windows.ContainsKey(Scene.Model.id))
                _windows.Remove(Scene.Model.id);
            AppSettings.Default.editorWindowWidth = this.Width;
            AppSettings.Default.editorWindowHeight = this.Height;
            AppSettings.Default.editorZoom = this.FindControl<Slider>("zoomSlider").Value;
            AppSettings.Default.Save();
        }

        public SceneViewModel Scene { get; private set; }

    }
}
