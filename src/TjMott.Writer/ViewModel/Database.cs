using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model;
using TjMott.Writer.Model.Scripts;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class Database : ViewModelBase, INotifyPropertyChanged
    {
        #region Private variables
        private SQLiteConnection _connection;
        private string _filename;
        private UniverseViewModel _selectedUniverse;
        #endregion

        #region Properties
        public SQLiteConnection Connection
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

        public static Database Instance { get; private set; }

        public SortBySortIndexBindingList<UniverseViewModel> Universes { get; private set; }
        public Metadata Metadata { get; private set; }
        #endregion

        #region ICommands
        private ICommand _createUniverseCommand;
        public ICommand CreateUniverseCommand
        {
            get
            {
                if (_createUniverseCommand == null)
                {
                    _createUniverseCommand = new RelayCommand(param => CreateUniverse());
                }
                return _createUniverseCommand;
            }
        }
        #endregion

        public Database(string filename)
        {
            _filename = filename;

            bool dbNeedsInitialization = !System.IO.File.Exists(filename);
            _connection = new SQLiteConnection(@"URI=file:" + filename);
            _connection.Open();
            _connection.EnableExtensions(true);
            _connection.LoadExtension("SQLite.Interop.dll", "sqlite3_fts5_init");

            using (var cmd = new SQLiteCommand(_connection))
            {
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
            }

            if (dbNeedsInitialization)
            {
                using (var cmd = new SQLiteCommand(_connection))
                {
                    cmd.CommandText = DbInitScript.InitScript;
                    cmd.ExecuteNonQuery();
                }

                Universe defaultUniverse = new Universe(_connection);
                defaultUniverse.Name = "New Universe";
                defaultUniverse.MarkdownCss = DefaultMarkdownCss.DefaultCss;
                defaultUniverse.Create();

                Metadata md = new Metadata(_connection);
                md.DefaultUniverseId = defaultUniverse.id;
            }

            Metadata = new Metadata(_connection);

            // Version check.
            if (Metadata.DbVersion != Metadata.ExpectedVersion)
            {
                // TODO: Database upgrade.
            }

            Universes = new SortBySortIndexBindingList<UniverseViewModel>();
            Instance = this;
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
            foreach (var u in Universes)
                u.SpellcheckDictionary.Close();

            if (Connection != null)
            {
                // Compact the database.
                using (SQLiteCommand cmd = new SQLiteCommand(Connection))
                {
                    cmd.CommandText = "VACUUM;";
                    cmd.ExecuteNonQuery();
                }
            }

            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        public void CreateUniverse()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, "New Universe");
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Universe uni = new Universe(_connection);
                uni.Name = dialog.UserInput;
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
