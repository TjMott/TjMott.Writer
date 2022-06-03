using CefNet.Avalonia;
using CefNet.JSInterop;
using SixLabors.Fonts;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.Controls
{
    public class QuillJsEditor : WebView
    {
        public event EventHandler EditorLoaded;
        public event EventHandler TextChanged;
        private static string editor_path;
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
                    SetJsonText(_document.Json);
                }
            }
        }

        static QuillJsEditor()
        {
            editor_path = "file:///" + Path.Join(Directory.GetCurrentDirectory(), "Assets", "editor.html");
        }

        public QuillJsEditor()
        {
            InitialUrl = editor_path;
            DocumentTitleChanged += QuillJsEditor_DocumentTitleChanged;
            IsEnabled = false;
        }

        private void QuillJsEditor_DocumentTitleChanged(object sender, CefNet.DocumentTitleChangedEventArgs e)
        {
            // Kind of a hack but it works. The easiest way to get an event from the JS side to the C# side is to change
            // the HTML document title, because that is already routed as a C# event. And the user never sees
            // the HTML title anyway.
            if (e.Title == "readyForInit" && !_isInitialized)
            {
                initEditor();
                _isInitialized = true; // This event likes to double-fire.
            }
            else if (e.Title == "loaded")
            {
                IsEnabled = true;
                if (_document != null)
                    SetJsonText(_document.Json);
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
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            string text = quill.getText();
            return text;
        }

        public async Task<string> GetJsonDocument()
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = quill.getContents();
            return delta;
        }

        public async Task<string> GetJsonText()
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = quill.getContents();
            string contents = window.JSON.stringify(delta, null, 4);
            return contents;
        }

        public async void SetJsonDocument(dynamic json)
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            quill.setContents(json);
        }

        public async void SetJsonText(string json)
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
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

        private async void initEditor()
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
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

        public async void Save()
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
            Document.Save();
        }

        public void Revert()
        {
            Document.Load();
            SetJsonText(Document.Json);
        }
    }
}
