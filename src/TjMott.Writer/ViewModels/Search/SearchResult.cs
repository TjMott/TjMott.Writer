using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace TjMott.Writer.ViewModels.Search
{
    public abstract class SearchResult : ViewModelBase
    {
        public long rowid { get; set; }
        public string Snippet { get; set; }
        public string SnippetPre { get; set; }
        public string SnippetResult { get; set; }
        public string SnippetPost { get; set; }
        public double Rank { get; set; }

        private string _resultType = "";
        public string ResultType
        {
            get { return _resultType; }
            protected set
            {
                _resultType = value;
                OnPropertyChanged("ResultType");
            }
        }

        private string _context = "";
        public string Context
        {
            get { return _context; }
            protected set
            {
                _context = value;
                OnPropertyChanged("Context");
            }
        }

        private string _name = "";
        public string Name
        {
            get { return _name; }
            protected set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public SearchResult(SqliteDataReader reader)
        {
            rowid = reader.GetInt64(0);
            Snippet = reader.GetString(1);
            Rank = reader.GetDouble(2);

            int resultStart = Snippet.IndexOf("<FTSSearchResult>");
            int resultEnd = Snippet.IndexOf("</FTSSearchResult>");
            SnippetPre = ProcessSnippet(Snippet.Substring(0, resultStart));
            SnippetResult = ProcessSnippet(Snippet.Substring(resultStart, resultEnd - resultStart));
            SnippetPost = ProcessSnippet(Snippet.Substring(resultEnd));
        }

        public static string ProcessSnippet(string s)
        {
            return s.Replace("<FTSSearchResult>", "")
                    .Replace("</FTSSearchResult>", "")
                    .Replace("\r\n", " ")
                    .Replace("\r", "")
                    .Replace("\n", "");
        }

        public ReactiveCommand<Window, Unit> RenameCommand { get; protected set; }

        public ReactiveCommand<Window, Unit> EditCommand { get; protected set; }
    }
}
