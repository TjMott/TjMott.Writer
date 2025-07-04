using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using SixLabors.Fonts;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

namespace TjMott.Writer.Controls
{
    public partial class QuillJsContainer : UserControl, IDisposable
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
                    setIsReadOnly(!value);
                }
            }
        }

        private DocumentInterop _documentInterop;
        private const string DOCUMENT_INTEROP_KEY = "DocumentInterop"; // Must match value in JS code.

        public QuillJsContainer()
        {
            InitializeComponent();
            internalInitialize();
        }

        private void internalInitialize()
        {
            _documentInterop = new DocumentInterop();
            _documentInterop.ReadyToInitialize += _documentInterop_ReadyToInitialize;
            _documentInterop.EditorLoaded += _documentInterop_EditorLoaded;
            _documentInterop.TextChanged += _documentInterop_TextChanged;

            _editor = new QuillJsEditor();
            _editor.IsVisible = false; // Start invisible to hide any flashing due to theme loading.

            _editor.RegisterJavascriptObject(_documentInterop, DOCUMENT_INTEROP_KEY, DocumentInterop.AsyncCallNativeMethod);
                
            webViewContainer.Children.Add(_editor);
            zoomSlider.PropertyChanged += zoomSlider_PropertyChanged;
        }

        private void updateEditorTheme()
        {
            if (Application.Current.ActualThemeVariant == ThemeVariant.Light)
                _editor.ExecuteJavaScript("enableLightTheme();");
            else
                _editor.ExecuteJavaScript("enableDarkTheme();");
        }

        private void Application_ActualThemeVariantChanged(object sender, EventArgs e)
        {
            updateEditorTheme();
        }

        private void _documentInterop_TextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
                TextChanged(this, new EventArgs());
            int wordCount = GetWordCount();
            wordCountTextBlock.Text = string.Format("Word Count: {0}", wordCount);
        }

        private void _documentInterop_EditorLoaded(object sender, EventArgs e)
        {
            IsEnabled = true;
            setIsReadOnly(true);
            if (_document != null)
                loadDocument();

            // Push theme down to JS app.
            updateEditorTheme();

            // Listen in case user changes theme while the editor exists.
            Application.Current.ActualThemeVariantChanged += Application_ActualThemeVariantChanged;

            if (!AllowUserEditing)
                _editor.ExecuteJavaScript("window.showToolbar(false);");
            _editor.IsVisible = true;

            if (EditorLoaded != null)
                EditorLoaded(this, new EventArgs());
        }

        private void _documentInterop_ReadyToInitialize(object sender, EventArgs e)
        {
            if (!_isInitialized)
            {
                initEditor();
                setTextZoom(ZoomLevel);
            }
        }

        private void zoomSlider_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Value")
            {
                _zoomLevel = (double)e.NewValue;
                if (_editor != null)
                    setTextZoom(_zoomLevel);
            }
        }

        /*public async Task<string> GetText()
        {
            return await _editor.EvaluateJavaScript<string>("return window.editor.getText();");
        }

        public async Task<object> GetJsonDocument()
        {
            return await _editor.EvaluateJavaScript<string>("return window.editor.getContents();");
        }

        public async Task<string> GetJsonText()
        {
            return await _editor.EvaluateJavaScript<string>("return window.JSON.stringify(window.editor.getContents(), null, 4);");
        }*/

        public void SetJsonDocument(dynamic jsonObject)
        {
            /*dynamic scriptableObject = await _editor.GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            quill.setContents(json);*/
        }
        

        public void SetJsonText(string jsonString)
        {
            // Update data in the interop object.
            _documentInterop.SetDocumentJson(jsonString);

            // Tell web app to retrieve and load the new document.
            string script = "(function() {" +
                DOCUMENT_INTEROP_KEY + ".getDocumentJson().then(result => window.editor.setContents(window.JSON.parse(result)));" +
                "})()";

            _editor.ExecuteJavaScript(script);
        }

        public int GetWordCount()
        {
            string text = _documentInterop.GetDocumentText();
            if (string.IsNullOrWhiteSpace(text)) return 0;
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

        private void initEditor()
        {
            _isInitialized = true;
            setTextZoom(ZoomLevel);

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

        private void setIsReadOnly(bool isReadOnly)
        {
            if (isReadOnly)
                _editor.ExecuteJavaScript("window.editor.disable();");
            else
                _editor.ExecuteJavaScript("window.editor.enable();");
            _editor.ExecuteJavaScript(string.Format("window.showToolbar({0});", !isReadOnly));
        }

        public async Task Save()
        {
            Document.PublicJson = _documentInterop.GetDocumentJson();
            if (Document.IsEncrypted)
            {
                Document.PlainText = "";
                Document.WordCount = 0;
            }
            else
            {
                Document.PlainText = _documentInterop.GetDocumentText();
                Document.WordCount = GetWordCount();
            }
            await Document.SaveAsync();
        }

        public async void Revert()
        {
            await Document.LoadAsync();
            SetJsonText(Document.PublicJson);
        }

        public async void Lock()
        {
            if (HasUnsavedEdits())
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
            await Task.Yield(); // Shut up compiler warnings for now.
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
            if (HasUnsavedEdits())
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

        public bool HasUnsavedEdits()
        {
            if (Document.IsUnlocked)
            {
                string editorCopy = _documentInterop.GetDocumentJson();
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

        private void setUnlockedState()
        {
            passwordTextBox.Text = "";
            aesPasswordContainer.IsVisible = false;
            webViewContainer.IsVisible = true;
            SetJsonText(Document.PublicJson);
            if (AllowUserEditing)
                setIsReadOnly(false);
            else
                setIsReadOnly(true);
        }

        private void setLockedState()
        {
            aesPasswordContainer.IsVisible = true;
            webViewContainer.IsVisible = false;
            passwordTextBox.Focus();
            setIsReadOnly(true);
            SetJsonText(Document.PublicJson);
        }

        private void setTextZoom(double zoom)
        {
            _editor.ExecuteJavaScript(string.Format("window.setTextZoom(\"{0}\");", zoom.ToString("P")));
            zoomTextBlock.Text = string.Format("Zoom: {0:P0}", zoom);
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

        public void Dispose()
        {
            Application.Current.ActualThemeVariantChanged -= Application_ActualThemeVariantChanged;
            if (_editor != null)
            {
                _editor.Dispose();
                _editor = null;
            }    
        }
    }
}
