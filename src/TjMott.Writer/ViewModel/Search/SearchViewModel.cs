using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using TjMott.Writer.Model;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel.Search
{
    public class SearchViewModel : ViewModelBase
    {
        #region SQLite stuff
        private static DbCommandHelper _cmdSearchFlowDocuments;
        private static DbCommandHelper _cmdSearchSceneNames;
        private static DbCommandHelper _cmdSearchChapterNames;
        private static DbCommandHelper _cmdSearchStoryNames;
        private static DbCommandHelper _cmdSearchMarkdownDocuments;

        private static void initSql(SQLiteConnection con)
        {
            if (_cmdSearchFlowDocuments == null)
            {
                _cmdSearchFlowDocuments = new DbCommandHelper(con);
                _cmdSearchFlowDocuments.Command.CommandText = @"SELECT 
                                                                    FlowDocument_fts.rowid, 
                                                                    snippet(FlowDocument_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                    rank
                                                                FROM 
                                                                    FlowDocument_fts
                                                                WHERE 
                                                                    FlowDocument_fts MATCH @searchTerm;";
                _cmdSearchFlowDocuments.AddParameter("@searchTerm");

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

                _cmdSearchMarkdownDocuments = new DbCommandHelper(con);
                _cmdSearchMarkdownDocuments.Command.CommandText = @"SELECT
                                                                        MarkdownDocument_fts.rowid, 
                                                                        snippet(MarkdownDocument_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25),
                                                                        rank
                                                                    FROM 
                                                                        MarkdownDocument_fts
                                                                    WHERE 
                                                                        MarkdownDocument_fts MATCH @searchTerm;";
                _cmdSearchMarkdownDocuments.AddParameter("@searchTerm");
            }
        }
        #endregion
        private UniverseViewModel _selectedUniverse;
        public UniverseViewModel SelectedUniverse
        {
            get { return _selectedUniverse; }
            set
            {
                _selectedUniverse = value;
                initSql(value.Model.Connection);
                OnPropertyChanged("SelectedUniverse");
            }
        }

        private string _searchTerm = "";
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                OnPropertyChanged("SearchTerm");
                DoSearch();
            }
        }

        private string _status = "";
        public string Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public BindingList<SearchResult> SearchResults { get; private set; }

        public SearchViewModel()
        {
            SearchResults = new BindingList<SearchResult>();
            Status = "No Results.";
        }

        public void DoSearch()
        {
            Status = "Performing Search...";
            SearchResults.Clear();
            List<SearchResult> results = new List<SearchResult>();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                
                doFlowDocumentSearch(results);
                doSceneTitleSearch(results);
                doChapterTitleSearch(results);
                doStoryTitleSearch(results);
                doMarkdownDocumentSearch(results);

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

        private void doFlowDocumentSearch(List<SearchResult> results)
        {
            _cmdSearchFlowDocuments.Parameters["@searchTerm"].Value = _searchTerm;
            using (SQLiteDataReader reader = _cmdSearchFlowDocuments.Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    FlowDocumentSearchResult result = new FlowDocumentSearchResult(reader); ;

                    // Search scenes for owner.
                    SceneViewModel scene = _selectedUniverse.Stories.SelectMany(i => i.Chapters.SelectMany(i => i.Scenes).Where(i => i.Model.FlowDocumentId == result.rowid)).SingleOrDefault();
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
        }

        private void doSceneTitleSearch(List<SearchResult> results)
        {

            _cmdSearchSceneNames.Parameters["@searchTerm"].Value = _searchTerm;

            using (SQLiteDataReader reader = _cmdSearchSceneNames.Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    SceneSearchResult result = new SceneSearchResult(reader);

                    // Search scenes for owner.
                    SceneViewModel scene = _selectedUniverse.Stories.SelectMany(i => i.Chapters.SelectMany(i => i.Scenes).Where(i => i.Model.id == result.rowid)).SingleOrDefault();
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
        }

        private void doChapterTitleSearch(List<SearchResult> results)
        {
            _cmdSearchChapterNames.Parameters["@searchTerm"].Value = _searchTerm;

            using (SQLiteDataReader reader = _cmdSearchChapterNames.Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ChapterSearchResult result = new ChapterSearchResult(reader);

                    // Search scenes for owner.
                    ChapterViewModel chapter = _selectedUniverse.Stories.SelectMany(i => i.Chapters.Where(i => i.Model.id == result.rowid)).SingleOrDefault();
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
        }

        private void doStoryTitleSearch(List<SearchResult> results)
        {
            _cmdSearchStoryNames.Parameters["@searchTerm"].Value = _searchTerm;

            using (SQLiteDataReader reader = _cmdSearchStoryNames.Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    StorySearchResult result = new StorySearchResult(reader);

                    // Search for owner.
                    StoryViewModel story = _selectedUniverse.Stories.SingleOrDefault(i => i.Model.id == result.rowid);
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
        }

        private void doMarkdownDocumentSearch(List<SearchResult> results)
        {
            _cmdSearchMarkdownDocuments.Parameters["@searchTerm"].Value = _searchTerm;

            using (SQLiteDataReader reader = _cmdSearchMarkdownDocuments.Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    MarkdownDocumentSearchResult result = new MarkdownDocumentSearchResult(reader);

                    MarkdownDocumentViewModel mdvm = _selectedUniverse.MarkdownTree.Items.SingleOrDefault(i => i is MarkdownDocumentViewModel && (i as MarkdownDocumentViewModel).Model.id == result.rowid) as MarkdownDocumentViewModel;
                    if (mdvm == null)
                    {
                        // "Special" documents don't have a viewmodel loaded at all times.
                        MarkdownDocument doc = new MarkdownDocument(_cmdSearchMarkdownDocuments.Connection);
                        doc.id = result.rowid;
                        doc.Load();
                        mdvm = new MarkdownDocumentViewModel(doc, this.SelectedUniverse);
                    }

                    if (mdvm != null)
                    {
                        // If the document is "special", it is attached to an item such as a ticket, chapter, scene, etc.
                        if (mdvm.Model.IsSpecial)
                        {
                            // Ticket search.
                            TicketViewModel ticketVm = _selectedUniverse.TicketTrackerViewModel.Tickets.SingleOrDefault(i => i.Model.MarkdownDocumentId == mdvm.Model.id);
                            if (ticketVm != null)
                            {
                                result.Owner = ticketVm;
                            }

                            // Category search.
                            CategoryViewModel catVm = _selectedUniverse.Categories.SingleOrDefault(i => i.Model.MarkdownDocumentId == mdvm.Model.id);
                            if (catVm != null)
                            {
                                result.Owner = catVm;
                            }

                            // Story search.
                            StoryViewModel storyVm = _selectedUniverse.Stories.SingleOrDefault(i => i.Model.MarkdownDocumentId == mdvm.Model.id);
                            if (storyVm != null)
                            {
                                result.Owner = storyVm;
                            }

                            // Chapter search.
                            ChapterViewModel chapterVm = _selectedUniverse.Stories.SelectMany(i => i.Chapters).SingleOrDefault(i => i.Model.MarkdownDocumentId == mdvm.Model.id);
                            if (chapterVm != null)
                            {
                                result.Owner = chapterVm;
                            }

                            // Scene search.
                            SceneViewModel sceneVm = _selectedUniverse.Stories.SelectMany(i => i.Chapters).SelectMany(i => i.Scenes).SingleOrDefault(i => i.Model.MarkdownDocumentId == mdvm.Model.id);
                            if (sceneVm != null)
                            {
                                result.Owner = sceneVm;
                            }
                        }
                        else
                        {
                            // Not special. This is a normal note in the universe's tree of markdown documents.
                            result.Owner = mdvm;
                        }

                        if (result.Owner != null)
                        {
                            results.Add(result);
                        }
                    }
                }
            }
        }
    }
}
