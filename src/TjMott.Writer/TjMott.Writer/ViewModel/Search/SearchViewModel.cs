using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using TjMott.Writer.Model;

namespace TjMott.Writer.ViewModel.Search
{
    public class SearchViewModel : ViewModelBase
    {
        #region SQLite stuff
        private static DbCommandHelper _cmdSearchFlowDocuments;
        private static DbCommandHelper _cmdSearchSceneNames;
        private static DbCommandHelper _cmdSearchChapterNames;
        private static DbCommandHelper _cmdSearchStoryNames;

        private static void initSql(SQLiteConnection con)
        {
            if (_cmdSearchFlowDocuments == null)
            {
                _cmdSearchFlowDocuments = new DbCommandHelper(con);
                _cmdSearchFlowDocuments.Command.CommandText = @"SELECT 
                                                                    FlowDocument_fts.rowid, 
                                                                    snippet(FlowDocument_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25)
                                                                FROM 
                                                                    FlowDocument_fts
                                                                WHERE 
                                                                    FlowDocument_fts MATCH @searchTerm
                                                                ORDER BY rank;";
                _cmdSearchFlowDocuments.AddParameter("@searchTerm");

                _cmdSearchSceneNames = new DbCommandHelper(con);
                _cmdSearchSceneNames.Command.CommandText = @"SELECT
                                                                Scene_fts.rowid, 
                                                                snippet(Scene_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25)
                                                             FROM 
                                                                Scene_fts
                                                             WHERE 
                                                                Scene_fts MATCH @searchTerm
                                                             ORDER BY rank;";
                _cmdSearchSceneNames.AddParameter("@searchTerm");

                _cmdSearchChapterNames = new DbCommandHelper(con);
                _cmdSearchChapterNames.Command.CommandText = @"SELECT
                                                                Chapter_fts.rowid, 
                                                                snippet(Chapter_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25)
                                                             FROM 
                                                                Chapter_fts
                                                             WHERE 
                                                                Chapter_fts MATCH @searchTerm
                                                             ORDER BY rank;";
                _cmdSearchChapterNames.AddParameter("@searchTerm");

                _cmdSearchStoryNames = new DbCommandHelper(con);
                _cmdSearchStoryNames.Command.CommandText = @"SELECT
                                                                Story_fts.rowid, 
                                                                snippet(Story_fts, -1, '<FTSSearchResult>', '</FTSSearchResult>', '', 25)
                                                             FROM 
                                                                Story_fts
                                                             WHERE 
                                                                Story_fts MATCH @searchTerm
                                                             ORDER BY rank;";
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

        public BindingList<FlowDocumentSearchResult> FlowDocumentResults { get; private set; }
        public BindingList<SceneSearchResult> SceneResults { get; private set; }
        public BindingList<ChapterSearchResult> ChapterResults { get; private set; }
        public BindingList<StorySearchResult> StoryResults { get; private set; }

        public SearchViewModel()
        {
            FlowDocumentResults = new BindingList<FlowDocumentSearchResult>();
            SceneResults = new BindingList<SceneSearchResult>();
            ChapterResults = new BindingList<ChapterSearchResult>();
            StoryResults = new BindingList<StorySearchResult>();
        }

        public void DoSearch()
        {
            doFlowDocumentSearch();
            doSceneTitleSearch();
            doChapterTitleSearch();
            doStoryTitleSearch();
        }

        private void doFlowDocumentSearch()
        {
            FlowDocumentResults.Clear();
            _cmdSearchFlowDocuments.Parameters["@searchTerm"].Value = _searchTerm;
            using (SQLiteDataReader reader = _cmdSearchFlowDocuments.Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    FlowDocumentSearchResult result = new FlowDocumentSearchResult(reader);

                    // Search scenes for owner.
                    SceneViewModel scene = _selectedUniverse.Stories.SelectMany(i => i.Chapters.SelectMany(i => i.Scenes).Where(i => i.Model.FlowDocumentId == result.rowid)).SingleOrDefault();
                    if (scene != null)
                    {
                        result.Owner = scene;
                        result.SceneName = scene.Model.Name;
                        result.ChapterName = scene.ChapterVm.Model.Name;
                        result.StoryName = scene.ChapterVm.StoryVm.Model.Name;
                    }

                    if (result.Owner != null)
                    {
                        FlowDocumentResults.Add(result);
                    }
                }
            }
        }

        private void doSceneTitleSearch()
        {
            SceneResults.Clear();
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
                        result.ChapterName = scene.ChapterVm.Model.Name;
                        result.StoryName = scene.ChapterVm.StoryVm.Model.Name;
                    }

                    if (result.Owner != null)
                    {
                        SceneResults.Add(result);
                    }
                }
            }
        }

        private void doChapterTitleSearch()
        {
            ChapterResults.Clear();
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
                        result.StoryName = chapter.StoryVm.Model.Name;
                    }

                    if (result.Owner != null)
                    {
                        ChapterResults.Add(result);
                    }
                }
            }
        }

        private void doStoryTitleSearch()
        {
            StoryResults.Clear();
            _cmdSearchStoryNames.Parameters["@searchTerm"].Value = _searchTerm;

            using (SQLiteDataReader reader = _cmdSearchStoryNames.Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    StorySearchResult result = new StorySearchResult(reader);

                    // Search scenes for owner.
                    StoryViewModel chapter = _selectedUniverse.Stories.SingleOrDefault(i => i.Model.id == result.rowid);
                    if (chapter != null)
                    {
                        result.Owner = chapter;
                    }

                    if (result.Owner != null)
                    {
                        StoryResults.Add(result);
                    }
                }
            }
        }

        public static string ProcessSnippet(string s)
        {
            return s.Replace("<FTSSearchResult>", "")
                    .Replace("</FTSSearchResult>", "")
                    .Replace("\r\n", " ")
                    .Replace("\r", "")
                    .Replace("\n", "");
        }
    }
}
