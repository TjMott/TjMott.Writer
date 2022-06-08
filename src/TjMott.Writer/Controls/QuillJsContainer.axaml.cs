using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CefNet.Avalonia;
using CefNet.JSInterop;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.Controls
{
    public partial class QuillJsContainer : UserControl
    {
        public event EventHandler EditorLoaded;
        public event EventHandler TextChanged;

        private QuillJsEditor _editor;
        private bool _isInitialized = false;

        private Document _document;
        public Document Document
        {
            get { return _document; }
            set
            {
                _document = value;
                if (IsInitialized)
                {
                    loadDocument().Wait();
                }
            }
        }

        private double _zoomLevel = AppSettings.Default.editorZoom;
        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set 
            { 
                _zoomLevel = value;
                if (_editor != null)
                {
                    _editor.ZoomLevel = value;
                }
            }
        }

        public QuillJsContainer()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (CefNetAppImpl.InitSuccess)
            {
                _editor = new QuillJsEditor();
                _editor.DocumentTitleChanged += _editor_DocumentTitleChanged;

                this.FindControl<Grid>("webViewContainer").Children.Add(_editor);
            }
            else
            {
                this.FindControl<Grid>("webViewContainer").Children.Add(new EditorCefErrorDisplay());
            }
        }

        private async void _editor_DocumentTitleChanged(object sender, CefNet.DocumentTitleChangedEventArgs e)
        {
            // Kind of a hack but it works. The easiest way to get an event from the JS side to the C# side is to change
            // the HTML document title, because that is already routed as a C# event. And the user never sees
            // the HTML title anyway.
            if (e.Title == "readyForInit" && !_isInitialized)
            {
                await initEditor().ConfigureAwait(false);
            }
            else if (e.Title == "loaded")
            {
                IsEnabled = true;
                if (_document != null)
                    await loadDocument().ConfigureAwait(false);
                if (EditorLoaded != null)
                    EditorLoaded(this, new EventArgs());
            }
            else if (e.Title == "TextChanged")
            {
                if (TextChanged != null)
                    TextChanged(this, new EventArgs());
            }
        }

        public async Task<string> GetText()
        {
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            string text = quill.getText();
            return text;
        }

        public async Task<string> GetJsonDocument()
        {
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = quill.getContents();
            return delta;
        }

        public async Task<string> GetJsonText()
        {
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = quill.getContents();
            string contents = window.JSON.stringify(delta, null, 4);
            return contents;
        }

        public async void SetJsonDocument(dynamic json)
        {
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            quill.setContents(json);
        }

        public async Task SetJsonText(string json)
        {
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = window.JSON.parse(json);
            quill.setContents(delta);
        }

        public async Task<int> GetWordCount()
        {
            string text = await GetText();
            string[] words = text.Split(new string[] { " ", "\r", "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return words.Length;
        }

        private async Task loadDocument()
        {
            if (Document.IsEncrypted)
            {
                this.FindControl<Grid>("aesPasswordContainer").IsVisible = true;
                this.FindControl<Grid>("webViewContainer").IsVisible = false;
            }
            else
            {
                this.FindControl<Grid>("aesPasswordContainer").IsVisible = false;
                this.FindControl<Grid>("webViewContainer").IsVisible = true;
                await SetJsonText(Document.Json).ConfigureAwait(false);
            }
        }

        private async Task initEditor()
        {
            _isInitialized = true;
            _editor.ZoomLevel = ZoomLevel;
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;

            // Enumerate installed fonts, add them to the Quill editor.
            FontCollection col = new FontCollection();
            col.AddSystemFonts();
            foreach (var font in col.Families.OrderBy(i => i.Name))
            {
                window.addFont(font.Name);
            }

            // Initialize Quilljs.
            window.initEditor();
        }

        public async Task Save()
        {
            Document.Json = await GetJsonText();
            if (Document.IsEncrypted)
            {
                Document.PlainText = "";
                Document.WordCount = 0;
            }
            else
            {
                Document.PlainText = await GetText();
                Document.WordCount = await GetWordCount();
            }
            await Document.SaveAsync().ConfigureAwait(false);
        }

        public async void Revert()
        {
            await Document.LoadAsync().ConfigureAwait(false);
            await SetJsonText(Document.Json).ConfigureAwait(false);
        }

        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
