using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using TjMott.Writer.Properties;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Windows
{
    /// <summary>
    /// Interaction logic for NoteDocumentWindow.xaml
    /// </summary>
    public partial class MarkdownDocumentWindow : Window
    {
        #region Instance stuff
        private static Dictionary<long, MarkdownDocumentWindow> _windows = new Dictionary<long, MarkdownDocumentWindow>();

        public static void ShowMarkdownDocument(MarkdownDocumentViewModel doc)
        {
            if (!_windows.ContainsKey(doc.Model.id))
            {
                _windows[doc.Model.id] = new MarkdownDocumentWindow();
                _windows[doc.Model.id]._viewModel = doc;
            }
            _windows[doc.Model.id].Show();
            _windows[doc.Model.id].Activate();
            _windows[doc.Model.id].Focus();
        }

        public static void CloseAllWindows()
        {
            foreach (var item in _windows)
            {
                item.Value.Close();
            }
        }
        #endregion

        private MarkdownDocumentViewModel _viewModel;

        private MarkdownDocumentWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;
            Width = AppSettings.Default.wikiBrowserWindowWidth;
            Height = AppSettings.Default.wikiBrowserWindowHeight;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _windows.Remove(_viewModel.Model.id);
            AppSettings.Default.wikiBrowserWindowHeight = ActualHeight;
            AppSettings.Default.wikiBrowserWindowWidth = ActualWidth;
            AppSettings.Default.Save();
        }

        private void closeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                _viewModel.Save();
            }
        }
    }
}
