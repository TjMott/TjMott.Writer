using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class EditCopyrightPageWindow : Window
    {
        #region Instancing stuff
        private static Dictionary<long, EditCopyrightPageWindow> _windows = new Dictionary<long, EditCopyrightPageWindow>();
        public static void ShowEditorWindow(StoryViewModel story)
        {
            try
            {
                if (!_windows.ContainsKey(story.Model.id))
                {
                    EditCopyrightPageWindow wnd = new EditCopyrightPageWindow(story);
                    _windows[story.Model.id] = wnd;
                }

                _windows[story.Model.id].Show();
                _windows[story.Model.id].Activate();
                _windows[story.Model.id].Focus();
            }
            catch (ApplicationException)
            {

            }
        }
        #endregion

        private Document _copyrightPage;
        public StoryViewModel Story { get; private set; }

        #region Unused constructor -- still needed to compile
        public EditCopyrightPageWindow()
        {
            InitializeComponent();
        }
        #endregion

        public EditCopyrightPageWindow(StoryViewModel story)
        {
            Story = story;
            InitializeComponent();
            internalInitialize();
        }

        private void internalInitialize()
        {
            if (!Avalonia.Controls.Design.IsDesignMode)
            {
                Title = "Copyright Page: " + Story.Model.Name;

                manuscriptEditor.EditorLoaded += _manuscriptEditor_EditorLoaded;
                this.Width = AppSettings.Default.editorWindowWidth;
                this.Height = AppSettings.Default.editorWindowHeight;
                Closing += EditCopyrightPageWindow_Closing;
                _copyrightPage = new Document(Story.Model.Connection);
                _copyrightPage.id = Story.Model.CopyrightPageId.Value;
                AddHandler(KeyDownEvent, onKeyDown);
                OpenWindowsViewModel.Instance.CopyrightWindows.Add(this);
            }
        }

        private async void _manuscriptEditor_EditorLoaded(object sender, EventArgs e)
        {
            manuscriptEditor.EditorLoaded -= _manuscriptEditor_EditorLoaded;
            manuscriptEditor.ZoomLevel = AppSettings.Default.editorZoom;
            await _copyrightPage.LoadAsync();
            manuscriptEditor.Document = _copyrightPage;
        }

        private async void EditCopyrightPageWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await Task.Yield(); // Shut up compiler warning for now.
            e.Cancel = true;
            if (manuscriptEditor.HasUnsavedEdits())
            {
                var msgBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Save Before Closing?",
                    "Your copyright page has unsaved edits. Save before closing?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNoCancel,
                    MsBox.Avalonia.Enums.Icon.Question,
                    WindowStartupLocation.CenterOwner);
                var msgBoxResult = await msgBox.ShowWindowDialogAsync(this);
                if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    setStatusText("Saving copyright page...", 0);
                    await manuscriptEditor.Save();
                    setStatusText("Copyright page saved.", 0);
                }
                else if (msgBoxResult == MsBox.Avalonia.Enums.ButtonResult.Cancel)
                {
                    setStatusText("Close canceled.", 5000);
                    return;
                }
            }

            Closing -= EditCopyrightPageWindow_Closing;
            if (_windows.ContainsKey(Story.Model.id))
                _windows.Remove(Story.Model.id);
            AppSettings.Default.editorWindowWidth = this.Width;
            AppSettings.Default.editorWindowHeight = this.Height;
            AppSettings.Default.editorZoom = manuscriptEditor.ZoomLevel;
            AppSettings.Default.Save();

            if (OpenWindowsViewModel.Instance.CopyrightWindows.Contains(this))
                OpenWindowsViewModel.Instance.CopyrightWindows.Remove(this);

            Close();
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            if (_copyrightPage.IsUnlocked)
            {
                manuscriptEditor.Print(string.Format("Copyright Page: {0}", Story.Model.Name));
            }
        }

        private async void onKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.S && e.KeyModifiers == Avalonia.Input.KeyModifiers.Control)
            {
                setStatusText("Saving copyright page...", 0);
                await manuscriptEditor.Save();
                setStatusText("Copyright page saved.", 5000);
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
