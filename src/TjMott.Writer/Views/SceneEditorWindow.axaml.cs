using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TjMott.Writer.Controls;
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
                    SceneEditorWindow wnd = new SceneEditorWindow();
                    wnd.Scene = scene;
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
        public SceneEditorWindow()
        {
            InitializeComponent();
            Initialized += SceneEditorWindow_Initialized;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void SceneEditorWindow_Initialized(object sender, EventArgs e)
        {
            Initialized -= SceneEditorWindow_Initialized;
            //this.FindControl<QuillJsEditor>("manuscriptEditor").SetJsonDocument(null);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public SceneViewModel Scene { get; private set; }
        
    }
}
