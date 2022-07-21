using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using TjMott.Writer.Models;

namespace TjMott.Writer.ViewModels.Search
{
    public class SearchViewModel : ViewModelBase
    {
        #region SQLite stuff
        private static DbCommandHelper _cmdSearchDocuments;
        private static DbCommandHelper _cmdSearchSceneNames;
        private static DbCommandHelper _cmdSearchChapterNames;
        private static DbCommandHelper _cmdSearchStoryNames;

        private static void initSql(SqliteConnection con)
        {
            if (_cmdSearchDocuments == null)
            {
                _cmdSearchDocuments = new DbCommandHelper(con);
                _cmdSearchDocuments.Command.CommandText = @"SELECT 
                                                                    Document_fts.rowid, 
                                                                    snippet(Document_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                    rank
                                                                FROM 
                                                                    Document_fts
                                                                WHERE 
                                                                    Document_fts MATCH @searchTerm;";
                _cmdSearchDocuments.AddParameter("@searchTerm");

                _cmdSearchSceneNames = new DbCommandHelper(con);
                _cmdSearchSceneNames.Command.CommandText = @"SELECT
                                                                Scene_fts.rowid, 
                                                                snippet(Scene_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                rank
                                                             FROM 
                                                                Scene_fts
                                                             WHERE 
                                                                Scene_fts MATCH @searchTerm;";
                _cmdSearchSceneNames.AddParameter("@searchTerm");

                _cmdSearchChapterNames = new DbCommandHelper(con);
                _cmdSearchChapterNames.Command.CommandText = @"SELECT
                                                                Chapter_fts.rowid, 
                                                                snippet(Chapter_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                rank
                                                             FROM 
                                                                Chapter_fts
                                                             WHERE 
                                                                Chapter_fts MATCH @searchTerm;";
                _cmdSearchChapterNames.AddParameter("@searchTerm");

                _cmdSearchStoryNames = new DbCommandHelper(con);
                _cmdSearchStoryNames.Command.CommandText = @"SELECT
                                                                Story_fts.rowid, 
                                                                snippet(Story_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                rank
                                                             FROM 
                                                                Story_fts
                                                             WHERE 
                                                                Story_fts MATCH @searchTerm;";
                _cmdSearchStoryNames.AddParameter("@searchTerm");

            }
        }
        #endregion

        private UniverseViewModel _selectedUniverse;
        public UniverseViewModel SelectedUniverse
        {
            get { return _selectedUniverse; }
            set
            {
                initSql(value.Model.Connection);
                this.RaiseAndSetIfChanged(ref _selectedUniverse, value);
            }
        }

        private string _searchTerm = "";
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                this.RaiseAndSetIfChanged(ref _searchTerm, value);
                DoSearch();
            }
        }

        private string _status = "";
        public string Status
        {
            get { return _status; }
            private set
            {
                this.RaiseAndSetIfChanged(ref _status, value);
            }
        }

        public ObservableCollection<SearchResult> SearchResults { get; private set; }

        public SearchViewModel()
        {
            SearchResults = new ObservableCollection<SearchResult>();
            Status = "No Results.";
        }

        public void DoSearch()
        {
            Status = "Performing Search...";
            SearchResults.Clear();
            List<SearchResult> results = new List<SearchResult>();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {

                doDocumentSearch(results);
                doSceneTitleSearch(results);
                doChapterTitleSearch(results);
                doStoryTitleSearch(results);

                results = results.OrderBy(i => i.Rank).ToList();
                foreach (var r in results)
                    SearchResults.Add(r);
            }
            if (results.Count == 0)
                Status = "No results found.";
            else if (results.Count == 1)
                Status = "Found 1 search result:";
            else
                Status = string.Format("Found {0} search results:", results.Count);
        }

        private void doDocumentSearch(List<SearchResult> results)
        {
        }

        private void doSceneTitleSearch(List<SearchResult> results)
        {

        }

        private void doChapterTitleSearch(List<SearchResult> results)
        {

        }

        private void doStoryTitleSearch(List<SearchResult> results)
        {

        }
    }
}
