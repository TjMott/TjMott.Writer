﻿using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
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
        private Universe _selectedUniverse;
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

        public Universe SelectedUniverse
        {
            get { return _selectedUniverse; }
        }

        private UniverseViewModel _selectedUniverseViewModel;
        public UniverseViewModel SelectedUniverseViewModel { get => _selectedUniverseViewModel; private set => this.RaiseAndSetIfChanged(ref _selectedUniverseViewModel, value); }  

        public bool RequiresUpgrade
        {
            get
            {
                return Metadata.DbVersion < Metadata.ExpectedVersion;
            }
        }

        private bool _isInTransaction = false;
        public bool IsInTransaction { get => _isInTransaction; private set => this.RaiseAndSetIfChanged(ref _isInTransaction, value); }

        public static Database Instance { get; private set; }

        public SortBySortIndexBindingList<Universe> Universes { get; private set; }
        public Metadata Metadata { get; private set; }
        #endregion

        #region Commands
        public ReactiveCommand<Window, Unit> CreateUniverseCommand { get; }
        public ReactiveCommand<Universe, Unit> SelectUniverseCommand { get; }
        public ReactiveCommand<Unit, Unit> StartTransactionCommand { get; }
        public ReactiveCommand<Unit, Unit> CommitTransactionCommand { get; }
        public ReactiveCommand<Unit, Unit> RollbackTransactionCommand { get; }
        #endregion

        public Database(string filename)
        {
            _filename = filename;

            bool dbNeedsInitialization = !System.IO.File.Exists(filename);

            _connection = new SqliteConnection(@"Data Source=" + filename);
            _connection.Open();
            _connection.EnableExtensions(true);

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
                defaultUniverse.Create();

                Metadata md = new Metadata(_connection);
                md.DefaultUniverseId = defaultUniverse.id;
            }

            Metadata = new Metadata(_connection);
            Universes = new SortBySortIndexBindingList<Universe>();
            Instance = this;

            CreateUniverseCommand = ReactiveCommand.CreateFromTask<Window>(CreateUniverse);
            SelectUniverseCommand = ReactiveCommand.CreateFromTask<Universe>(SelectUniverse);
            StartTransactionCommand = ReactiveCommand.CreateFromTask(startTransaction, this.WhenAnyValue(i => i.IsInTransaction, (bool v) => !v));
            CommitTransactionCommand = ReactiveCommand.CreateFromTask(commitTransaction, this.WhenAnyValue(i => i.IsInTransaction, (bool v) => v));
            RollbackTransactionCommand = ReactiveCommand.CreateFromTask(rollbackTransaction, this.WhenAnyValue(i => i.IsInTransaction, (bool v) => v));
        }

        public async Task SelectUniverse(Universe universe)
        {
            _selectedUniverse = universe;
            Metadata.DefaultUniverseId = _selectedUniverse.id;
            OnPropertyChanged("SelectedUniverse");
            await LoadSelectedUniverseAsync();
        }

        public async Task LoadAsync()
        {
            Universes.Clear();

            // Load universes.
            var universes = await Universe.LoadAll(_connection);
            foreach (var u in universes)
                Universes.Add(u);

            var defaultUniverse = universes.SingleOrDefault(i => i.id == Metadata.DefaultUniverseId);
            if (defaultUniverse != null)
                await SelectUniverse(defaultUniverse);
        }

        public async Task LoadSelectedUniverseAsync()
        {
            SelectedUniverseViewModel = new UniverseViewModel(SelectedUniverse, this);

            // Load database entities on background threads.
            List<Task> loadingTasks = new List<Task>();
            Task<List<CategoryViewModel>> loadCategoriesTask = Task.Run(async () => (await Category.LoadAll(_connection)).Where(i => i.UniverseId == SelectedUniverse.id).Select(i => new CategoryViewModel(i)).ToList());
            Task<List<StoryViewModel>> loadStoriesTask = Task.Run(async () => (await Story.LoadAll(_connection)).Where(i => i.UniverseId == SelectedUniverse.id).Select(i => new StoryViewModel(i)).ToList());
            Task<List<Chapter>> loadChaptersTask = Task.Run(async () => (await Chapter.LoadAll(_connection)).ToList());
            Task<List<Scene>> loadScenesTask = Task.Run(async () => (await Scene.GetAllScenes(_connection)).ToList());

            loadingTasks.Add(loadCategoriesTask);
            loadingTasks.Add(loadStoriesTask);
            loadingTasks.Add(loadChaptersTask);
            loadingTasks.Add(loadScenesTask);
            await Task.WhenAll(loadingTasks);

            var categories = loadCategoriesTask.Result;
            var stories = loadStoriesTask.Result;
            var chapters = loadChaptersTask.Result;
            var scenes = loadScenesTask.Result;

            // Link up objects.
            foreach (var cat in categories)
            {
                var catStories = stories.Where(i => i.Model.CategoryId == cat.Model.id);
                foreach (var ss in catStories)
                {
                    cat.Stories.Add(ss);
                }

                SelectedUniverseViewModel.Categories.Add(cat);
                SelectedUniverseViewModel.SubItems.Add(cat);
                cat.UniverseVm = SelectedUniverseViewModel;
            }

            foreach (var s in stories)
            {
                SelectedUniverseViewModel.Stories.Add(s);
                s.UniverseVm = SelectedUniverseViewModel;
                SelectedUniverseViewModel.UpdateStoryInTree(s);

                foreach (var c in chapters)
                {
                    if (c.StoryId == s.Model.id)
                    {
                        ChapterViewModel cvm = new ChapterViewModel(c);
                        cvm.StoryVm = s;
                        s.Chapters.Add(cvm);

                        foreach (var scene in scenes)
                        {
                            if (scene.ChapterId == c.id)
                            {
                                SceneViewModel svm = new SceneViewModel(scene);
                                svm.ChapterVm = cvm;
                                cvm.Scenes.Add(svm);
                            }
                        }

                    }
                }
            }
            
        }

        public void Close()
        {
            if (Connection != null)
            {
                Vacuum();
            }

            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        public async Task CreateUniverse(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow("New Universe");
            string result = await dialog.ShowDialog<string>(owner);
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
                    uni.SortIndex = Universes.Max(i => i.SortIndex) + 1;
                }
                await uni.CreateAsync().ConfigureAwait(false);

                Universes.Add(uni);

                if (Universes.Count == 1)
                    SelectUniverse(uni);
            }
        }

        private async Task startTransaction()
        {
            if (_isInTransaction) return;

            using (var cmd = new SqliteCommand("BEGIN TRANSACTION;", Connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            IsInTransaction = true;
        }

        private async Task commitTransaction()
        {
            if (!_isInTransaction) return;

            using (var cmd = new SqliteCommand("COMMIT TRANSACTION;", Connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            IsInTransaction = false;
        }

        private async Task rollbackTransaction()
        {
            if (!_isInTransaction) return;

            using (var cmd = new SqliteCommand("ROLLBACK TRANSACTION;", Connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }
            await LoadAsync();

            IsInTransaction = false;
        }

        public void Vacuum()
        {
            if (Connection != null)
            {
                // Compact the database.
                using (SqliteCommand cmd = new SqliteCommand("VACUUM;", Connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}
