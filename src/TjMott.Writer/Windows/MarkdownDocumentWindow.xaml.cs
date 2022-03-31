using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            fontSizeSlider.Value = AppSettings.Default.markdownDocEditorFontSize;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _windows.Remove(_viewModel.Model.id);
            AppSettings.Default.wikiBrowserWindowHeight = ActualHeight;
            AppSettings.Default.wikiBrowserWindowWidth = ActualWidth;
            AppSettings.Default.markdownDocEditorFontSize = (int)fontSizeSlider.Value;
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

        private void MarkdownReferenceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.markdownguide.org/cheat-sheet";
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
