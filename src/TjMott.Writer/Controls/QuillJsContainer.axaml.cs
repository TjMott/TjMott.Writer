using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CefNet.JSInterop;
using SixLabors.Fonts;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

namespace TjMott.Writer.Controls
{
    public partial class QuillJsContainer : UserControl
    {
        public event EventHandler EditorLoaded;
        public event EventHandler TextChanged;

        private QuillJsEditor _editor;
        private bool _isInitialized = false;

        public static readonly DirectProperty<QuillJsContainer, Document> DocumentProperty =
            AvaloniaProperty.RegisterDirect<QuillJsContainer, Document>(nameof(Document), o => o.Document, (o, v) => o.Document = v);
        private Document _document;
        public Document Document
        {
            get { return _document; }
            set
            {
                SetAndRaise(DocumentProperty, ref _document, value);
                if (_isInitialized && _document != null)
                {
                    loadDocument();
                }
                IsVisible = Document != null;
            }
        }

        private static readonly DirectProperty<QuillJsContainer, double> ZoomLevelProperty =
            AvaloniaProperty.RegisterDirect<QuillJsContainer, double>(nameof(ZoomLevel), o => o.ZoomLevel, (o, v) => o.ZoomLevel = v);
        private double _zoomLevel = 1.0;
        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set 
            { 
                SetAndRaise(ZoomLevelProperty, ref _zoomLevel, value);
                this.FindControl<Slider>("zoomSlider").Value = _zoomLevel;
            }
        }

        private static readonly DirectProperty<QuillJsContainer, bool> AllowUserEditingProperty =
            AvaloniaProperty.RegisterDirect<QuillJsContainer, bool>(nameof(AllowUserEditing), o => o.AllowUserEditing, (o, v) => o.AllowUserEditing = v);
        private bool _allowUserEditing = true;
        public bool AllowUserEditing
        {
            get { return _allowUserEditing; }
            set 
            { 
                SetAndRaise(AllowUserEditingProperty, ref _allowUserEditing, value);
                if (_editor != null)
                {
                    _ = setIsReadOnly(!value);
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
                this.FindControl<Slider>("zoomSlider").PropertyChanged += zoomSlider_PropertyChanged;
            }
            else if (!CefNetAppImpl.IsCefInstalled)
            {
                this.FindControl<Grid>("webViewContainer").Children.Add(new CefNotInstalledContainer());
            }
            else
            {
                this.FindControl<Grid>("webViewContainer").Children.Add(new EditorCefErrorDisplay());
            }
        }

        private void zoomSlider_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Value")
            {
                _zoomLevel = (double)e.NewValue;
                if (_editor != null)
                    _ = setTextZoom(_zoomLevel);
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
                await setTextZoom(ZoomLevel);
            }
            else if (e.Title == "loaded")
            {
                IsEnabled = true;
                await setIsReadOnly(true);
                if (_document != null)
                    loadDocument();
                if (EditorLoaded != null)
                    EditorLoaded(this, new EventArgs());
            }
            else if (e.Title == "TextChanged")
            {
                if (TextChanged != null)
                    TextChanged(this, new EventArgs());
                int wordCount = await GetWordCount();
                this.FindControl<TextBlock>("wordCountTextBlock").Text = string.Format("Word Count: {0}", wordCount);
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
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            quill.setContents(json);
        }

        public async Task SetJsonText(string json)
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = window.JSON.parse(json);
            quill.setContents(delta);
        }

        public async Task<int> GetWordCount()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return 0;
            }
            string text = await GetText();
            string[] words = text.Split(new string[] { " ", "\r", "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return words.Length;
        }

        private void loadDocument()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            if (Document.IsEncrypted && !Document.IsUnlocked)
            {
                setLockedState();
            }
            else
            {
                setUnlockedState();
            }
        }

        private async Task initEditor()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            _isInitialized = true;
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            await setTextZoom(ZoomLevel);

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

        private async Task setIsReadOnly(bool isReadOnly)
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            if (isReadOnly)
                quill.disable();
            else
                quill.enable();
            window.showToolbar(!isReadOnly);
        }

        public async Task Save()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            Document.PublicJson = await GetJsonText();
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
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            await Document.LoadAsync().ConfigureAwait(false);
            await SetJsonText(Document.PublicJson).ConfigureAwait(false);
        }

        public async void Lock()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            if (await HasUnsavedEdits())
            {
                var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Save Before Locking?",
                    "Your document has unsaved edits. Save before locking? You will lose your changes if you answer no!",
                    MessageBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                    MessageBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation.CenterOwner);
                var msgBoxResult = await msgBox.ShowDialog(getOwner());
                if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    await Save();
                }
                else if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.Cancel)
                {
                    return;
                }
            }
            Document.Lock();
            setLockedState();
        }

        public async void Print(string title)
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            if (_editor != null && _document.IsUnlocked)
            {
                dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
                dynamic window = scriptableObject.window;
                window.setPrintTitle(title);
                _editor.Print();
            }
        }

        

        public async void Encrypt()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            if (await HasUnsavedEdits())
            {
                var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Document is Unsaved",
                        "Your document has unsaved changes. Save these changes before encrypting?",
                        MessageBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                        MessageBox.Avalonia.Enums.Icon.Question,
                        WindowStartupLocation.CenterOwner);
                var msgBoxResult = await msgBox.Show(getOwner());
                if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    await Save();
                }
                else if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.No)
                {

                }
                else if (msgBoxResult == MessageBox.Avalonia.Enums.ButtonResult.Cancel)
                {
                    return;
                }
            }
            EncryptWindow wnd = new EncryptWindow();
            string password = await wnd.ShowDialog<string>(getOwner());
            if (!string.IsNullOrEmpty(password))
            {
                await Document.Encrypt(password);
                if (Document.IsEncrypted)
                {
                    setLockedState();
                }
            }
        }
        public async void Decrypt()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            DecryptWindow wnd = new DecryptWindow();
            string password = await wnd.ShowDialog<string>(getOwner());
            if (!string.IsNullOrEmpty(password))
            {
                try
                {
                    await Document.Decrypt(password);
                    if (!Document.IsEncrypted)
                    {
                        setUnlockedState();
                    }
                }
                catch (CryptographicException)
                {
                    var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Invalid Password",
                        "Incorrect password, your document could not be decrypted.",
                        MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        MessageBox.Avalonia.Enums.Icon.Error,
                        WindowStartupLocation.CenterOwner);
                    await msgBox.Show(getOwner());
                }
            }
        }

        public async Task<bool> HasUnsavedEdits()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return false;
            }
            if (Document.IsUnlocked)
            {
                string editorCopy = await GetJsonText().ConfigureAwait(true);
                return editorCopy != Document.PublicJson;
            }
            return false;
        }

        private void unlockButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            unlock();
        }

        private void passwordBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                unlock();
            }
        }

        private async void unlock()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            if (Document.IsEncrypted && !Document.IsUnlocked)
            {
                string password = this.FindControl<TextBox>("passwordTextBox").Text;
                try
                {
                    Document.Unlock(password);
                    setUnlockedState();
                }
                catch (CryptographicException)
                {
                    this.FindControl<TextBox>("passwordTextBox").Text = "";
                    var msgBox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Invalid Password",
                        "Incorrect password, your document could not be unlocked.",
                        MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                        MessageBox.Avalonia.Enums.Icon.Error,
                        WindowStartupLocation.CenterOwner);
                    await msgBox.Show(getOwner());

                }
            }
        }

        private async void setUnlockedState()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            this.FindControl<TextBox>("passwordTextBox").Text = "";
            this.FindControl<Grid>("aesPasswordContainer").IsVisible = false;
            this.FindControl<Grid>("webViewContainer").IsVisible = true;
            await SetJsonText(Document.PublicJson);
            if (AllowUserEditing)
                await setIsReadOnly(false);
            else
                await setIsReadOnly(true);
        }

        private async void setLockedState()
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            this.FindControl<Grid>("aesPasswordContainer").IsVisible = true;
            this.FindControl<Grid>("webViewContainer").IsVisible = false;
            this.FindControl<TextBox>("passwordTextBox").Focus();
            await setIsReadOnly(true);
            await SetJsonText(Document.PublicJson);
        }

        private async Task setTextZoom(double zoom)
        {
            if (!CefNetAppImpl.InitSuccess)
            {
                return;
            }
            dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            window.setTextZoom(zoom.ToString("P"));
            Dispatcher.UIThread.Post(() => this.FindControl<TextBlock>("zoomTextBlock").Text = string.Format("Zoom: {0:P0}", zoom));
        }

        private Window getOwner()
        {
            IControl parent = this.Parent;
            while (parent != null && parent is not Window)
            {
                parent = parent.Parent;
            }
            return parent as Window;
        }
        
    }
}
