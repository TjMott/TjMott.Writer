using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using TjMott.Writer.Models;

namespace TjMott.Writer.ViewModels.Search
{
    public class SearchViewModel : ViewModelBase
    {
        #region SQLite stuff
        private static DbCommandHelper _cmdSearchDocumentText;
        private static DbCommandHelper _cmdSearchSceneNames;
        private static DbCommandHelper _cmdSearchChapterNames;
        private static DbCommandHelper _cmdSearchStoryNames;
        private static DbCommandHelper _cmdSearchNoteNames;

        private static void initSql(SqliteConnection con)
        {
            if (_cmdSearchDocumentText == null)
            {
                _cmdSearchDocumentText = new DbCommandHelper(con);
                _cmdSearchDocumentText.Command.CommandText = @"SELECT 
                                                                    Document_fts.rowid, 
                                                                    snippet(Document_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                    rank
                                                                FROM 
                                                                    Document_fts
                                                                WHERE 
                                                                    Document_fts MATCH @searchTerm;";
                _cmdSearchDocumentText.AddParameter("@searchTerm");

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

                _cmdSearchNoteNames = new DbCommandHelper(con);
                _cmdSearchNoteNames.Command.CommandText = @"SELECT
                                                                NoteDocument_fts.rowid, 
                                                                snippet(NoteDocument_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                rank
                                                             FROM 
                                                                NoteDocument_fts
                                                             WHERE 
                                                                NoteDocument_fts MATCH @searchTerm;";
                _cmdSearchNoteNames.AddParameter("@searchTerm");

            }
        }
        #endregion

        private UniverseViewModel _universe;

        private string _searchTerm = "";
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                this.RaiseAndSetIfChanged(ref _searchTerm, value);
                _ = DoSearch();
            }
        }

        private string sqlSearchTerm
        {
            get
            {
                // Reformat for Sqlite FTS syntax.
                string temp = _searchTerm;

                string[] words = temp.Split(new char[] { ' ', '\'', '"', '.', '?' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string retval = "";
                for (int i = 0; i < words.Length - 1; i++)
                {
                    retval += words[i] + "*+";
                }
                retval += words[words.Length - 1] + "*";
                return retval;
            }
        }

        private string _status = "";
        public string Status { get => _status; private set => this.RaiseAndSetIfChanged(ref _status, value); }

        private SearchResult _selectedResult;
        public SearchResult SelectedResult { get => _selectedResult; set => this.RaiseAndSetIfChanged(ref _selectedResult, value); }

        public ObservableCollection<SearchResult> SearchResults { get; private set; }

        public SearchViewModel(UniverseViewModel universe)
        {
            _universe = universe;
            SearchResults = new ObservableCollection<SearchResult>();
            Status = "No Results.";
            initSql(_universe.Model.Connection);
        }

        private List<CancellationTokenSource> _searchCancellationTokens = new List<CancellationTokenSource>();
        public async Task DoSearch()
        {
            // This function can be re-entered if the user types faster than the search occurs.
            // Keep track of all runs and cancel them all.
            // TODO: Is this still true?
            if (_searchCancellationTokens.Count > 0)
            {
                foreach (var token in _searchCancellationTokens)
                    token.Cancel();
                while (_searchCancellationTokens.Count > 0)
                    await Task.Delay(1);
            }
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            _searchCancellationTokens.Add(cancelToken);
            try
            {
                Status = "Performing Search...";
                SearchResults.Clear();
                List<SearchResult> results = new List<SearchResult>();

                // Load the universe, otherwise the owner mapping can't be created.
                foreach (var storyVm in _universe.Stories)
                {
                    if (storyVm.Chapters.Count == 0)
                        await storyVm.LoadChapters();
                }

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    string searchTerm = sqlSearchTerm;

                    // Kick off search tasks on background thread.
                    await Task.Run(async () =>
                    {
                        results.AddRange(await doDocumentSearchAsync(searchTerm, cancelToken.Token));
                        results.AddRange(await doSceneTitleSearchAsync(searchTerm, cancelToken.Token));
                        results.AddRange(await doChapterTitleSearchAsync(searchTerm, cancelToken.Token));
                        results.AddRange(await doStoryTitleSearchAsync(searchTerm, cancelToken.Token));

                        // Sort by search ranking.
                        results = results.OrderBy(i => i.Rank).ToList();
                    });

                    // Add to UI-bound collection.
                    foreach (var r in results)
                    {
                        SearchResults.Add(r);
                    }

                }
                if (results.Count == 0)
                    Status = "No results found.";
                else if (results.Count == 1)
                    Status = "Found 1 search result:";
                else
                    Status = string.Format("Found {0} search results:", results.Count);
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
            finally
            {
                cancelToken.Dispose();
                _searchCancellationTokens.Remove(cancelToken);
            }
        }

        private async Task<List<SearchResult>> doDocumentSearchAsync(string searchTerm, CancellationToken cancelToken)
        {
            List<SearchResult> results = new List<SearchResult>();

            _cmdSearchDocumentText.Parameters["@searchTerm"].Value = searchTerm;
            using (SqliteDataReader reader = await _cmdSearchDocumentText.Command.ExecuteReaderAsync(cancelToken))
            {
                while (await reader.ReadAsync(cancelToken))
                {
                    DocumentTextSearchResult result = new DocumentTextSearchResult(reader);

                    // Search for owner.
                    SceneViewModel scene = _universe.Stories.SelectMany(i => i.Chapters.SelectMany(i => i.Scenes)).SingleOrDefault(i => i.Model.DocumentId == result.rowid);
                    if (scene != null)
                        result.Owner = scene;


                    if (result.Owner == null)
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        StoryViewModel story = _universe.Stories.SingleOrDefault(i => i.Model.CopyrightPageId == result.rowid);
                        if (story != null)
                            result.Owner = story;
                    }

                    if (result.Owner != null)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        private async Task<List<SearchResult>> doSceneTitleSearchAsync(string searchTerm, CancellationToken cancelToken)
        {
            List<SearchResult> results = new List<SearchResult>();

            _cmdSearchSceneNames.Parameters["@searchTerm"].Value = searchTerm;

            using (SqliteDataReader reader = await _cmdSearchSceneNames.Command.ExecuteReaderAsync(cancelToken))
            {
                while (await reader.ReadAsync(cancelToken))
                {
                    SceneTitleSearchResult result = new SceneTitleSearchResult(reader);

                    // Search scenes for owner.
                    SceneViewModel scene = _universe.Stories.SelectMany(i => i.Chapters.SelectMany(i => i.Scenes)).SingleOrDefault(i => i.Model.id == result.rowid);
                    if (scene != null)
                    {
                        result.Owner = scene;
                    }

                    if (result.Owner != null)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }


        private async Task<List<SearchResult>> doChapterTitleSearchAsync(string searchTerm, CancellationToken cancelToken)
        {
            List<SearchResult> results = new List<SearchResult>();

            _cmdSearchChapterNames.Parameters["@searchTerm"].Value = searchTerm;

            using (SqliteDataReader reader = await _cmdSearchChapterNames.Command.ExecuteReaderAsync(cancelToken))
            {
                while (await reader.ReadAsync(cancelToken))
                {
                    ChapterTitleSearchResult result = new ChapterTitleSearchResult(reader);

                    // Search scenes for owner.
                    ChapterViewModel chapter = _universe.Stories.SelectMany(i => i.Chapters.Where(i => i.Model.id == result.rowid)).SingleOrDefault();
                    if (chapter != null)
                    {
                        result.Owner = chapter;
                    }

                    if (result.Owner != null)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        private async Task<List<SearchResult>> doStoryTitleSearchAsync(string searchTerm, CancellationToken cancelToken)
        {
            List<SearchResult> results = new List<SearchResult>();

            _cmdSearchStoryNames.Parameters["@searchTerm"].Value = searchTerm;

            using (SqliteDataReader reader = await _cmdSearchStoryNames.Command.ExecuteReaderAsync(cancelToken))
            {
                while (await reader.ReadAsync(cancelToken))
                {
                    StoryTitleSearchResult result = new StoryTitleSearchResult(reader);

                    // Search for owner.
                    StoryViewModel story = _universe.Stories.SingleOrDefault(i => i.Model.id == result.rowid);
                    if (story != null)
                    {
                        result.Owner = story;
                    }

                    if (result.Owner != null)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

    }
}
