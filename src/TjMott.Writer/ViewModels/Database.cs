using Microsoft.Data.Sqlite;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Models.SqlScripts;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels
{
    public class Database : ViewModelBase
    {
        #region Private variables
        private SqliteConnection _connection;
        private string _filename;
        private UniverseViewModel _selectedUniverse;
        #endregion

        #region Properties
        public SqliteConnection Connection
        {
            get { return _connection; }
        }

        public string FileName
        {
            get { return _filename; }
        }

        public UniverseViewModel SelectedUniverse
        {
            get { return _selectedUniverse; }
            set
            {
                if (_selectedUniverse != value)
                {
                    _selectedUniverse = value;
                    _selectedUniverse.IsSelected = true;
                    Metadata.DefaultUniverseId = _selectedUniverse.Model.id;
                    foreach (var u in Universes)
                    {
                        if (u != _selectedUniverse)
                            u.IsSelected = false;
                    }
                    OnPropertyChanged("SelectedUniverse");
                }
            }
        }

        public bool RequiresUpgrade
        {
            get
            {
                return Metadata.DbVersion < Metadata.ExpectedVersion;
            }
        }

        public static Database Instance { get; private set; }

        public SortBySortIndexBindingList<UniverseViewModel> Universes { get; private set; }
        public Metadata Metadata { get; private set; }
        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> CreateUniverseCommand { get; }
        #endregion

        public Database(string filename)
        {
            _filename = filename;

            bool dbNeedsInitialization = !System.IO.File.Exists(filename);

            _connection = new SqliteConnection(@"Data Source=" + filename);
            _connection.Open();
            _connection.EnableExtensions(true);
            //_connection.LoadExtension("SQLite.Interop.dll", "sqlite3_fts5_init");

            using (var cmd = new SqliteCommand("PRAGMA foreign_keys = ON;", _connection))
            {
                cmd.ExecuteNonQuery();
            }

            if (dbNeedsInitialization)
            {
                using (var cmd = new SqliteCommand(DbInitScript.InitScript, _connection))
                {
                    cmd.ExecuteNonQuery();
                }

                Universe defaultUniverse = new Universe(_connection);
                defaultUniverse.Name = "New Universe";
                defaultUniverse.MarkdownCss = "";//DefaultMarkdownCss.DefaultCss;
                defaultUniverse.Create();

                Metadata md = new Metadata(_connection);
                md.DefaultUniverseId = defaultUniverse.id;
            }

            Metadata = new Metadata(_connection);
            Universes = new SortBySortIndexBindingList<UniverseViewModel>();
            Instance = this;

            CreateUniverseCommand = ReactiveCommand.Create(CreateUniverse);
        }

        public void Load()
        {
            // Load database entities and create viewmodels.
            var universes = Universe.GetAllUniverses(_connection).Select(i => new UniverseViewModel(i, this)).ToList();
            var categories = Category.GetAllSeries(_connection).Select(i => new CategoryViewModel(i)).ToList();
            var stories = Story.GetAllStories(_connection).Select(i => new StoryViewModel(i)).ToList();
            var chapters = Chapter.GetAllChapters(_connection).Select(i => new ChapterViewModel(i)).ToList();
            var scenes = Scene.GetAllScenes(_connection).Select(i => new SceneViewModel(i)).ToList();

            // Link up objects.
            foreach (var chapter in chapters)
            {
                var chapterScenes = scenes.Where(i => i.Model.ChapterId == chapter.Model.id);
                foreach (var scene in chapterScenes)
                {
                    scene.ChapterVm = chapter;
                    chapter.Scenes.Add(scene);
                }
            }

            foreach (var story in stories)
            {
                var storyChapters = chapters.Where(i => i.Model.StoryId == story.Model.id);
                foreach (var chapter in storyChapters)
                {
                    chapter.StoryVm = story;
                    story.Chapters.Add(chapter);
                }
            }

            foreach (var cat in categories)
            {
                var catStories = stories.Where(i => i.Model.CategoryId == cat.Model.id);
                foreach (var ss in catStories)
                {
                    cat.Stories.Add(ss);
                }
            }

            foreach (var u in universes)
            {
                var cats = categories.Where(i => i.Model.UniverseId == u.Model.id);
                foreach (var cat in cats)
                {
                    u.Categories.Add(cat);
                    u.SubItems.Add(cat);
                    cat.UniverseVm = u;
                }

                var stories1 = stories.Where(i => i.Model.UniverseId == u.Model.id);
                foreach (var s in stories1)
                {
                    u.Stories.Add(s);
                    s.UniverseVm = u;
                    u.UpdateStoryInTree(s);
                }

                Universes.Add(u);
            }

            long defaultUniverseId = Metadata.DefaultUniverseId;
            UniverseViewModel defaultUniverse = Universes.SingleOrDefault(i => i.Model.id == defaultUniverseId);
            if (defaultUniverse != null)
                SelectedUniverse = defaultUniverse;
        }

        public void Close()
        {
            //foreach (var u in Universes)
            //    u.SpellcheckDictionary.Close();

            if (Connection != null)
            {
                // Compact the database.
                using (SqliteCommand cmd = new SqliteCommand("VACUUM;", Connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        public async void CreateUniverse()
        {
            NameItemWindow dialog = new NameItemWindow("New Universe");
            string result = await dialog.ShowDialog<string>(MainWindow);
            if (!string.IsNullOrWhiteSpace(result))
            {
                Universe uni = new Universe(_connection);
                uni.Name = result;
                if (Universes.Count == 0)
                {
                    uni.SortIndex = 0;
                }
                else
                {
                    uni.SortIndex = Universes.Max(i => i.Model.SortIndex) + 1;
                }
                uni.Create();
                UniverseViewModel vm = new UniverseViewModel(uni, this);
                Universes.Add(vm);

                if (Universes.Count == 1)
                    SelectedUniverse = vm;
            }
        }

    }
}
