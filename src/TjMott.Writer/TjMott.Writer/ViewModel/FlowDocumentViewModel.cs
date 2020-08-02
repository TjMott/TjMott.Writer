using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml;
using sql = TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class FlowDocumentViewModel : ViewModelBase
    {
        #region Private variables
        private FlowDocument _flowDocument;
        private string _status = "";
        private bool _hasChanges = false;
        private long _wordCount;
        private string _searchTerm = "";
        private string _currentKeyword = "";
        private TextRange _selectedSearchResult;
        private Visibility _searchResultsVisibility = Visibility.Collapsed;
        private Visibility _noSearchResultsVisibility = Visibility.Collapsed;
        #endregion

        #region ICommands
        private ICommand _saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(param => Save());
                }
                return _saveCommand;
            }
        }
        private ICommand _revertCommand;
        public ICommand RevertCommand
        {
            get
            {
                if (_revertCommand == null)
                {
                    _revertCommand = new RelayCommand(param => Revert());
                }
                return _revertCommand;
            }
        }
        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(param => Close());
                }
                return _closeCommand;
            }
        }
        private ICommand _exportCommand;
        public ICommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new RelayCommand(param => Export());
                }
                return _exportCommand;
            }
        }
        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new RelayCommand(param => Search(), param => CanSearch());
                }
                return _searchCommand;
            }
        }
        private ICommand _jumpToFirstSearchResultCommand;
        public ICommand JumpToFirstSearchResultCommand
        {
            get
            {
                if (_jumpToFirstSearchResultCommand == null)
                {
                    _jumpToFirstSearchResultCommand = new RelayCommand(param => GoToFirstSearchResult(), param => CanGoToFirstSearchResult());
                }
                return _jumpToFirstSearchResultCommand;
            }
        }
        private ICommand _jumpToNextSearchResultCommand;
        public ICommand JumpToNextSearchResultCommand
        {
            get
            {
                if (_jumpToNextSearchResultCommand == null)
                {
                    _jumpToNextSearchResultCommand = new RelayCommand(param => GoToNextSearchResult(), param => CanGoToLastSearchResult());
                }
                return _jumpToNextSearchResultCommand;
            }
        }
        private ICommand _jumpToLastSearchResultCommand;
        public ICommand JumpToLastSearchResultCommand
        {
            get
            {
                if (_jumpToLastSearchResultCommand == null)
                {
                    _jumpToLastSearchResultCommand = new RelayCommand(param => GoToLastSearchResult(), param => CanGoToLastSearchResult());
                }
                return _jumpToLastSearchResultCommand;
            }
        }
        private ICommand _jumpToPreviousSearchResultCommand;
        public ICommand JumpToPreviousSearchResultCommand
        {
            get
            {
                if (_jumpToPreviousSearchResultCommand == null)
                {
                    _jumpToPreviousSearchResultCommand = new RelayCommand(param => GoToPreviousSearchResult(), param => CanGoToFirstSearchResult());
                }
                return _jumpToPreviousSearchResultCommand;
            }
        }
        private ICommand _closeSearchCommand;
        public ICommand CloseSearchCommand
        {
            get
            {
                if (_closeSearchCommand == null)
                {
                    _closeSearchCommand = new RelayCommand(param => CloseSearch());
                }
                return _closeSearchCommand;
            }
        }
        #endregion

        #region Properties
        public sql.FlowDocument Model { get; private set; }
        public Window Owner { get; private set; }

        public FlowDocument Document
        {
            get { return _flowDocument; }
            set
            {
                _flowDocument = value;
                OnPropertyChanged("Document");
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }
        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                _hasChanges = value;
                WordCount = WordCounter.GetWordCount(Document);
                OnPropertyChanged("HasChanges");
            }
        }

        public long WordCount
        {
            get { return _wordCount; }
            private set
            {
                _wordCount = value;
                OnPropertyChanged("WordCount");
            }
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                OnPropertyChanged("SearchTerm");
            }
        }

        public TextRange SelectedSearchResult
        {
            get { return _selectedSearchResult; }
            private set
            {
                _selectedSearchResult = value;
                OnPropertyChanged("SelectedSearchResult");
                OnPropertyChanged("SelectedSearchResultIndex");
                OnPropertyChanged("SearchResultCount");
            }
        }

        public int SelectedSearchResultIndex
        {
            get { return SearchResults.IndexOf(_selectedSearchResult) + 1; }
        }

        public int SearchResultCount
        {
            get { return SearchResults.Count; }
        }

        public Visibility SearchResultsVisibility
        {
            get { return _searchResultsVisibility; }
            private set
            {
                _searchResultsVisibility = value;
                OnPropertyChanged("SearchResultsVisibility");
            }
        }

        public Visibility NoSearchResultsVisibility
        {
            get { return _noSearchResultsVisibility; }
            private set
            {
                _noSearchResultsVisibility = value;
                OnPropertyChanged("NoSearchResultsVisibility");
            }
        }

        public BindingList<TextRange> SearchResults { get; private set; }
        #endregion

        public FlowDocumentViewModel(sql.FlowDocument model, Window owner)
        {
            Model = model;
            Owner = owner;
            WordCount = Model.WordCount;
            SearchResults = new BindingList<TextRange>();
            loadFlowDocument();
        }

        public void Save()
        {
            Model.Xml = XamlWriter.Save(Document);
            Model.PlainText = getRawText(Document);
            Model.WordCount = WordCounter.GetWordCount(Document);
            Model.Save();
            Status = "Document saved.";
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (o, e) =>
            {
                timer.Stop();
                Status = "";
            };
            timer.Start();
            HasChanges = false;
        }

        public void Revert()
        {
            loadFlowDocument();
            Status = "Document reverted.";
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (o, e) =>
            {
                timer.Stop();
                Status = "";
            };
            timer.Start();
            HasChanges = false;
        }

        public void Close()
        {
            Owner.Close();
        }

        public void Export()
        {

        }

        public void Search()
        {
            if (SearchTerm == _currentKeyword)
            {
                if (CanGoToLastSearchResult())
                    GoToNextSearchResult();
                return;
            }

            SearchResults.Clear();
            _currentKeyword = SearchTerm;

            TextRange text = new TextRange(Document.ContentStart, Document.ContentEnd);
            TextPointer current = text.Start.GetInsertionPosition(LogicalDirection.Backward);
            while (current != null)
            {
                string textInRun = current.GetTextInRun(LogicalDirection.Forward);
                if (!string.IsNullOrWhiteSpace(textInRun))
                {
                    int index = 0;
                    index = textInRun.IndexOf(SearchTerm, index);
                    while (index != -1)
                    {
                        TextPointer selectionStart = current.GetPositionAtOffset(index, LogicalDirection.Forward);
                        TextPointer selectionEnd = selectionStart.GetPositionAtOffset(SearchTerm.Length, LogicalDirection.Forward);
                        TextRange selection = new TextRange(selectionStart, selectionEnd);
                        SearchResults.Add(selection);
                        index = textInRun.IndexOf(SearchTerm, index + 1);
                    }
                }

                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }

            if (SearchResults.Count == 0)
            {
                NoSearchResultsVisibility = Visibility.Visible;
                SearchResultsVisibility = Visibility.Collapsed;
            }
            else
            {
                NoSearchResultsVisibility = Visibility.Collapsed;
                SearchResultsVisibility = Visibility.Visible;
                SelectedSearchResult = SearchResults.First();
            }
        }

        public bool CanSearch()
        {
            return !string.IsNullOrWhiteSpace(SearchTerm);
        }

        public void CloseSearch()
        {
            SearchResults.Clear();
            SearchTerm = "";
            _currentKeyword = "";
            NoSearchResultsVisibility = Visibility.Collapsed;
            SearchResultsVisibility = Visibility.Collapsed;
        }

        public void GoToFirstSearchResult()
        {
            SelectedSearchResult = SearchResults.First();
        }

        public void GoToNextSearchResult()
        {
            int index = SearchResults.IndexOf(SelectedSearchResult);
            SelectedSearchResult = SearchResults[index + 1];
        }

        public bool CanGoToFirstSearchResult()
        {
            return SearchResults.IndexOf(SelectedSearchResult) > 0;
        }

        public void GoToPreviousSearchResult()
        {
            int index = SearchResults.IndexOf(SelectedSearchResult);
            SelectedSearchResult = SearchResults[index - 1];
        }

        public void GoToLastSearchResult()
        {
            SelectedSearchResult = SearchResults.Last();
        }

        public bool CanGoToLastSearchResult()
        {
            return SearchResults.IndexOf(SelectedSearchResult) < SearchResults.Count - 1;
        }

        private void loadFlowDocument()
        {
            using (StringReader stringReader = new StringReader(Model.Xml))
            {
                using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                {
                    Document = (FlowDocument)XamlReader.Load(xmlReader);
                    setDocumentStyle(Document);
                }
            }
        }

        #region Static funtions
        public static string GetEmptyFlowDocXml()
        {
            FlowDocument doc = new FlowDocument();
            setDocumentStyle(doc);
            return XamlWriter.Save(doc);
        }

        private static void setDocumentStyle(FlowDocument doc)
        {
            doc.FontSize = 12;
            doc.FontFamily = new System.Windows.Media.FontFamily("Garamond");
            doc.LineHeight = 1.2;
        }

        private static string getRawText(FlowDocument doc)
        {
            TextRange tr = new TextRange(doc.ContentStart, doc.ContentEnd);
            return tr.Text;
        }
        #endregion

        #region Inner classes
        private static class WordCounter
        {
            public static int GetWordCount(FlowDocument doc)
            {
                int wordCount = 0;

                foreach (var block in doc.Blocks)
                {
                    wordCount += GetWordCount(block);
                }

                return wordCount;
            }

            static int GetWordCount(Block block)
            {
                if (block is Paragraph)
                {
                    return GetWordCount((Paragraph)block);
                }
                else if (block is Section)
                {
                    return GetWordCount((Section)block);
                }
                else if (block is List)
                {
                    return GetWordCount((List)block);
                }
                else if (block is Table)
                {
                    // TODO. Not sure if really needed.
                }

                return 0;
            }

            static int GetWordCount(Section section)
            {
                int wordCount = 0;
                foreach (var block in section.Blocks)
                {
                    wordCount += GetWordCount(block);
                }
                return wordCount;
            }

            static int GetWordCount(List list)
            {
                int wordCount = 0;
                foreach (var item in list.ListItems)
                {
                    foreach (var block in item.Blocks)
                    {
                        wordCount += GetWordCount(block);
                    }
                }
                return wordCount;
            }

            static int GetWordCount(Paragraph paragraph)
            {
                int wordCount = 0;
                foreach (var inline in paragraph.Inlines)
                {
                    wordCount += GetWordCount(inline);
                }
                return wordCount;
            }

            static int GetWordCount(Inline inline)
            {
                if (inline is Run)
                {
                    return GetWordCount((Run)inline);
                }
                else if (inline is Span)
                {
                    return GetWordCount((Span)inline);
                }

                return 0;
            }

            static int GetWordCount(Run run)
            {
                return GetWordCount(run.Text);
            }

            static int GetWordCount(Span span)
            {
                int wordcount = 0;
                foreach (var inline in span.Inlines)
                {
                    wordcount += GetWordCount(inline);
                }
                return wordcount;
            }

            static int GetWordCount(string text)
            {
                return text.Split(new char[] { ' ', '\t', '\n', '\r', '-' }, StringSplitOptions.RemoveEmptyEntries).Count(i => !string.IsNullOrWhiteSpace(i));
            }
        }
        #endregion
    }
}
