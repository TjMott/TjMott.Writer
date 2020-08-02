using System;
using System.Data.SQLite;
using System.Windows.Input;

namespace TjMott.Writer.ViewModel.Search
{
    public class ChapterSearchResult : ViewModelBase
    {
        private ICommand _renameItemCommand;
        public ICommand RenameItemCommand
        {
            get
            {
                if (_renameItemCommand == null)
                {
                    _renameItemCommand = new RelayCommand(param => RenameItem());
                }
                return _renameItemCommand;
            }
        }

        #region Properties
        public string StoryName { get; set; }
        public ChapterViewModel Owner { get; set; }
        public long rowid { get; set; }
        public string Snippet { get; set; }
        public string SnippetPre { get; set; }
        public string SnippetResult { get; set; }
        public string SnippetPost { get; set; }
        #endregion

        public ChapterSearchResult(SQLiteDataReader reader)
        {
            rowid = reader.GetInt64(0);
            Snippet = reader.GetString(1);

            int resultStart = Snippet.IndexOf("<FTSSearchResult>");
            int resultEnd = Snippet.IndexOf("</FTSSearchResult>");
            SnippetPre = SearchViewModel.ProcessSnippet(Snippet.Substring(0, resultStart));
            SnippetResult = SearchViewModel.ProcessSnippet(Snippet.Substring(resultStart, resultEnd - resultStart));
            SnippetPost = SearchViewModel.ProcessSnippet(Snippet.Substring(resultEnd));
        }

        public void RenameItem()
        {
            Owner.Rename();
        }
    }
}
