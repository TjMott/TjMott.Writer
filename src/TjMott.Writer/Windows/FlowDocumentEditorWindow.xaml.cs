using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TjMott.Writer.Properties;
using TjMott.Writer.ViewModel;
using sql = TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.Windows
{
    /// <summary>
    /// Interaction logic for FlowDocumentEditorWindow.xaml
    /// </summary>
    public partial class FlowDocumentEditorWindow : Window
    {
        #region Instancing stuff
        private static Dictionary<long, FlowDocumentEditorWindow> _windows = new Dictionary<long, FlowDocumentEditorWindow>();

        public static void ShowEditorWindow(long flowDocId, SQLiteConnection connection, SpellcheckDictionary spellcheckDictionary, string title, string search = null)
        {
            if (!_windows.ContainsKey(flowDocId))
            {
                sql.FlowDocument flowDoc = new sql.FlowDocument(connection);
                flowDoc.id = flowDocId;
                flowDoc.Load();
                FlowDocumentEditorWindow wnd = new FlowDocumentEditorWindow(flowDoc);
                wnd._spellcheckDictionary = spellcheckDictionary;

                _windows[flowDocId] = wnd;
            }
            _windows[flowDocId].Title = title;
            _windows[flowDocId].Show();
            _windows[flowDocId].Activate();
            _windows[flowDocId].Focus();

            if (!string.IsNullOrWhiteSpace(search))
                _windows[flowDocId].PerformSearch(search);
        }
        #endregion

        private SpellcheckDictionary _spellcheckDictionary;
        private FlowDocumentViewModel _viewModel;
        public FlowDocumentEditorWindow(sql.FlowDocument doc)
        {
            InitializeComponent(); 
            _viewModel = new FlowDocumentViewModel(doc, this);
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;
            zoomSlider.Value = AppSettings.Default.editorZoom;
            Width = AppSettings.Default.editorWidth;
            Height = AppSettings.Default.editorHeight;

            MainTextBox.Document = _viewModel.Document;
            MainTextBox.SpellCheck.CustomDictionaries.Add(_spellcheckDictionary.GetDictionaryUri());
            _spellcheckDictionary.DictionaryModified += _spellcheckDictionary_DictionaryModified;

            initFontCombos();
        }

        private void initFontCombos()
        {
            List<double> fontSizes = new List<double>();
            fontSizes.Add(8);
            fontSizes.Add(9);
            fontSizes.Add(10);
            fontSizes.Add(11);
            fontSizes.Add(12);
            fontSizes.Add(14);
            fontSizes.Add(16);
            fontSizes.Add(18);
            fontSizes.Add(20);
            fontSizes.Add(22);
            fontSizes.Add(24);
            fontSizes.Add(26);
            fontSizes.Add(28);
            fontSizes.Add(36);
            fontSizes.Add(48);
            fontSizes.Add(72);
            fontSizes.Add(96);
            fontSizeGalleryCategory.ItemsSource = fontSizes;

            fontGalleryCategory.ItemsSource = Fonts.SystemFontFamilies;
        }

        private void _spellcheckDictionary_DictionaryModified(object sender, EventArgs e)
        {
            MainTextBox.SpellCheck.CustomDictionaries.Clear();
            MainTextBox.SpellCheck.CustomDictionaries.Add(_spellcheckDictionary.GetDictionaryUri());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel != null && _viewModel.HasChanges)
            {
                try
                {
                    // Throws ObjectDisposedException if DB is closed.
                    ConnectionState state = _viewModel.Model.Connection.State;

                    MessageBoxResult result = MessageBox.Show("Save changes before closing?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                    else if (result == MessageBoxResult.Yes)
                        _viewModel.Save();
                }
                catch (ObjectDisposedException)
                { }
            }
            // _viewModel may be null if this was an encrypted entry and the wrong password was entered.
            if (_viewModel != null)
            {
                AppSettings.Default.editorHeight = ActualHeight;
                AppSettings.Default.editorWidth = ActualWidth;
                AppSettings.Default.editorZoom = zoomSlider.Value;
                AppSettings.Default.Save();
                _windows.Remove(_viewModel.Model.id);
            }
        }

        private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Changes.Count > 0)
                _viewModel.HasChanges = true;
            foreach (var change in e.Changes)
            {
                if (change.AddedLength > 0)
                {
                    TextRange tr = new TextRange(MainTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset), MainTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength));
                    if (tr.Text == "'")
                    {
                        TextRange prev = new TextRange(tr.Start.GetPositionAtOffset(-1), tr.Start);
                        if (string.IsNullOrWhiteSpace(prev.Text))
                        {
                            tr.Text = "‘";
                        }
                        else
                        {
                            tr.Text = "’";
                        }
                        MainTextBox.CaretPosition = tr.End;
                    }
                    else if (tr.Text == "-")
                    {
                        TextRange prev = new TextRange(tr.Start.GetPositionAtOffset(-1), tr.Start);
                        if (prev.Text == "-")
                        {
                            prev.Text = "";
                            tr.Text = "—";
                            MainTextBox.CaretPosition = tr.End;
                        }
                    }
                    else if (tr.Text == ".")
                    {
                        TextRange prev = new TextRange(tr.Start.GetPositionAtOffset(-1), tr.Start);
                        TextRange prev2 = new TextRange(prev.Start.GetPositionAtOffset(-1), prev.Start);
                        if (prev.Text == "." && prev2.Text == ".")
                        {
                            prev.Text = "";
                            prev2.Text = "";
                            tr.Text = "…";
                            MainTextBox.CaretPosition = tr.End;
                        }
                    }
                    else if (tr.Text.StartsWith("\""))
                    {
                        TextRange prev = new TextRange(tr.Start.GetPositionAtOffset(-1), tr.Start);
                        if (string.IsNullOrWhiteSpace(prev.Text))
                        {
                            // Sometimes leading quotes are followed by \r\n. Not sure why.
                            tr.Text = "“" + tr.Text.Substring(1);
                        }
                        else
                        {
                            tr.Text = "”" + tr.Text.Substring(1);
                        }
                        MainTextBox.CaretPosition = tr.End;
                    }
                }
            }
        }

        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                _viewModel.Save();
                //System.Windows.Documents.EditingCommands.IncreaseIndentation;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                findTextBox.SelectAll();
                findTextBox.Focus();
            }
        }

        public void PerformSearch(string keyword)
        {
            findTextBox.Text = keyword;
            _viewModel.Search();
        }

        private void findTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.Search();
            }
        }

        private void _viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedSearchResult")
            {
                TextRange tr = _viewModel.SelectedSearchResult;
                if (tr != null)
                {
                    MainTextBox.Selection.Select(tr.Start, tr.End);
                    TextPointer t = MainTextBox.Selection.Start;
                    Rect r = t.GetCharacterRect(LogicalDirection.Backward);
                    mainScrollViewer.ScrollToVerticalOffset(r.Y * MainTextBoxScaleTransform.ScaleY);
                    MainTextBox.Focus();
                }
            }
        }

        #region Spellchecker & Thesaurus
        private void SpellCheckCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RibbonCheckBox checkbox = sender as RibbonCheckBox;
            if (checkbox != null && MainTextBox != null) // Can happen on initial XAML load.
                SpellCheck.SetIsEnabled(MainTextBox, checkbox.IsChecked.Value);
        }
        private void MainTextBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            MainTextBox.ContextMenu.Items.Clear();
            SpellingError spellingError = MainTextBox.GetSpellingError(MainTextBox.CaretPosition);
            if (spellingError != null && spellingError.Suggestions.Count() > 0)
            {
                foreach (string suggestion in spellingError.Suggestions)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = suggestion;
                    menuItem.FontWeight = FontWeights.Bold;
                    menuItem.Command = EditingCommands.CorrectSpellingError;
                    menuItem.CommandParameter = suggestion;
                    menuItem.CommandTarget = MainTextBox;
                    MainTextBox.ContextMenu.Items.Add(menuItem);
                }
            }
            else if (spellingError != null)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = "No Suggestions";
                menuItem.IsEnabled = false;
                MainTextBox.ContextMenu.Items.Add(menuItem);
            }
            if (spellingError != null)
            {
                Separator separator = new Separator();
                MainTextBox.ContextMenu.Items.Add(separator);

                MenuItem addToDictionary = new MenuItem();
                addToDictionary.Header = "Add to Dictionary";
                var tr = MainTextBox.GetSpellingErrorRange(MainTextBox.CaretPosition);
                addToDictionary.Click += (object o, RoutedEventArgs args) =>
                {
                    _spellcheckDictionary.AddToDictionary(tr.Text);
                };
                MainTextBox.ContextMenu.Items.Add(addToDictionary);
            }
            else
            {
                e.Handled = true;
            }
        }

        #endregion

        private void fontFamilyComboBox_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FontFamily font = (FontFamily)e.NewValue;
            MainTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, font);
            _viewModel.HasChanges = true;
        }

        private void fontSizeComboBox_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            double fontSize = (double)e.NewValue;
            MainTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            _viewModel.HasChanges = true;
        }

        private void MainTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            FontFamily font = (FontFamily)MainTextBox.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            fontFamilyComboBox.SelectedItem = font;

            double fontSize = (double)MainTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            fontSizeComboBox.SelectedItem = fontSize;
        }
    }
}
