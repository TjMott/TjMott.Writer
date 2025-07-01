using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SixLabors.Fonts;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
                zoomSlider.Value = _zoomLevel;
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
            internalInitialize();
        }

        private void internalInitialize()
        {
            //if (CefNetAppImpl.InitSuccess)
            {
                _editor = new QuillJsEditor();
                _editor.TitleChanged += _editor_DocumentTitleChanged;
                
                webViewContainer.Children.Add(_editor);
                zoomSlider.PropertyChanged += zoomSlider_PropertyChanged;
                _editor.ShowDeveloperTools();
            }
            /*else if (!CefNetAppImpl.IsCefInstalled)
            {
                webViewContainer.Children.Add(new CefNotInstalledContainer());
            }
            else
            {
                webViewContainer.Children.Add(new EditorCefErrorDisplay());
            }*/
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

        private async void _editor_DocumentTitleChanged(object sender, string title)
        {
            // Kind of a hack but it works. The easiest way to get an event from the JS side to the C# side is to change
            // the HTML document title, because that is already routed as a C# event. And the user never sees
            // the HTML title anyway.
            System.Diagnostics.Debug.WriteLine($"Browser title changed to {title}.");
            if (title == "readyForInit" && !_isInitialized)
            {
                await initEditor();
                await setTextZoom(ZoomLevel);
            }
            else if (title == "loaded")
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    IsEnabled = true;
                    await setIsReadOnly(true);
                    if (_document != null)
                        loadDocument();
                    if (EditorLoaded != null)
                        EditorLoaded(this, new EventArgs());
                });
            }
            else if (title == "TextChanged")
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (TextChanged != null)
                        TextChanged(this, new EventArgs());
                    int wordCount = await GetWordCount();
                    wordCountTextBlock.Text = string.Format("Word Count: {0}", wordCount);
                });
            }
        }

        public async Task<string> GetText()
        {
            return await _editor.EvaluateJavaScript<string>("editor.getText();");
        }

        public async Task<object> GetJsonDocument()
        {
            return await _editor.EvaluateJavaScript<string>("editor.getContents();");
        }

        public async Task<string> GetJsonText()
        {
            return await _editor.EvaluateJavaScript<string>("window.JSON.stringify(editor.getContents(), null, 4);");
        }

        public async void SetJsonDocument(dynamic json)
        {
            /*dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            quill.setContents(json);*/
        }

        public async Task SetJsonText(string json)
        {
            // Ugh. Escaping things.
            // Match double quotes not preceded by a backslash, and add a backslash.
            // TODO: This doesn't work yet. Content does not show in the Quill control.
            Regex quoteEscaper = new Regex("(?<!\\\\)\"");
            json = quoteEscaper.Replace(json, "\\\"");
            _editor.ExecuteJavaScript(string.Format("window.editor.setContents(window.JSON.parse(\"{0}\"));", json));
        }

        public async Task<int> GetWordCount()
        {
            string text = await GetText();
            string[] words = text.Split(new string[] { " ", "\r", "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return words.Length;
        }

        private void loadDocument()
        {
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
            _isInitialized = true;
            await setTextZoom(ZoomLevel);

            // Enumerate installed fonts, add them to the Quill editor.
            FontCollection col = new FontCollection();
            col.AddSystemFonts();
            foreach (var font in col.Families.OrderBy(i => i.Name))
            {
                _editor.ExecuteJavaScript(string.Format("window.addFont(\"{0}\");", font.Name));
            }

            // Initialize Quilljs.
            _editor.ExecuteJavaScript("window.initEditor();");
        }

        private async Task setIsReadOnly(bool isReadOnly)
        {
            if (isReadOnly)
                _editor.ExecuteJavaScript("window.editor.disable();");
            else
                _editor.ExecuteJavaScript("window.editor.enable();");
            _editor.ExecuteJavaScript(string.Format("window.showToolbar({0});", !isReadOnly));
        }

        public async Task Save()
        {
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
            await Document.LoadAsync().ConfigureAwait(false);
            await SetJsonText(Document.PublicJson).ConfigureAwait(false);
        }

        public async void Lock()
        {
            if (await HasUnsavedEdits())
            {
                var msgBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Save Before Locking?",
                    "Your document has unsaved edits. Save before locking? You will lose your changes if you answer no!",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                    MsBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation.CenterOwner);
                var msgBoxResult = await msgBox.ShowWindowDialogAsync(getOwner());
                if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    await Save();
                }
                else if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Cancel)
                {
                    return;
                }
            }
            Document.Lock();
            setLockedState();
        }

        public async void Print(string title)
        {
            /*if (_editor != null && _document.IsUnlocked)
            {
                dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
                dynamic window = scriptableObject.window;
                window.setPrintTitle(title);
                _editor.Print();
            }*/
        }

        

        public async void Encrypt()
        {
            if (await HasUnsavedEdits())
            {
                var msgBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Document is Unsaved",
                        "Your document has unsaved changes. Save these changes before encrypting?",
                        MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                        MsBox.Avalonia.Enums.Icon.Question,
                        WindowStartupLocation.CenterOwner);
                var msgBoxResult = await msgBox.ShowWindowDialogAsync(getOwner());
                if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    await Save();
                }
                else if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.No)
                {

                }
                else if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Cancel)
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
                    var msgBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Invalid Password",
                        "Incorrect password, your document could not be decrypted.",
                        MsBox.Avalonia.Enums.ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Error,
                        WindowStartupLocation.CenterOwner);
                    await msgBox.ShowWindowDialogAsync(getOwner());
                }
            }
        }

        public async Task<bool> HasUnsavedEdits()
        {
            if (Document.IsUnlocked)
            {
                string editorCopy = await GetJsonText().ConfigureAwait(true);
                return editorCopy != Document.PublicJson;
            }
            return false;
        }

        private void unlockButton_Click(object sender, RoutedEventArgs e)
        {
            unlock();
        }

        private void passwordBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                unlock();
            }
        }

        private async void unlock()
        {
            if (Document.IsEncrypted && !Document.IsUnlocked)
            {
                string password = passwordTextBox.Text;
                try
                {
                    Document.Unlock(password);
                    setUnlockedState();
                }
                catch (CryptographicException)
                {
                    passwordTextBox.Text = "";
                    var msgBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Invalid Password",
                        "Incorrect password, your document could not be unlocked.",
                        MsBox.Avalonia.Enums.ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Error,
                        WindowStartupLocation.CenterOwner);
                    await msgBox.ShowWindowDialogAsync(getOwner());

                }
            }
        }

        private async void setUnlockedState()
        {
            passwordTextBox.Text = "";
            aesPasswordContainer.IsVisible = false;
            webViewContainer.IsVisible = true;
            await SetJsonText(Document.PublicJson);
            if (AllowUserEditing)
                await setIsReadOnly(false);
            else
                await setIsReadOnly(true);
        }

        private async void setLockedState()
        {
            aesPasswordContainer.IsVisible = true;
            webViewContainer.IsVisible = false;
            passwordTextBox.Focus();
            await setIsReadOnly(true);
            await SetJsonText(Document.PublicJson);
        }

        private async Task setTextZoom(double zoom)
        {
            _editor.ExecuteJavaScript(string.Format("window.setTextZoom(\"{0}\");", zoom.ToString("P")));
            Dispatcher.UIThread.Post(() => zoomTextBlock.Text = string.Format("Zoom: {0:P0}", zoom));
        }

        private Window getOwner()
        {
            var parent = this.Parent;
            while (parent != null && parent is not Window)
            {
                parent = parent.Parent;
            }
            return parent as Window;
        }
        
    }
}
