using Avalonia.Controls;
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
            // Default SQLite behavior is one transaction per query. Each transaction creates a temp file. This means
            // performing a ton of queries will be slowed down by filesystem access. So try to wrap the database's
            // entire session in a transaction to reduce this overhead.
            await startTransaction();
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

            // Load database entities.
            var categoryModels = await Category.LoadForUniverse(SelectedUniverse.id, _connection);
            var categories = categoryModels.Select(i => new CategoryViewModel(i)).ToList();
            var storyModels = await Story.LoadForUniverse(SelectedUniverse.id, _connection);
            var stories = storyModels.Select(i => new StoryViewModel(i)).ToList();

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
            }
        }

        public async Task Close()
        {
            if (Connection != null)
            {
                await commitTransaction();
                await Vacuum();
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }            
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
                await uni.CreateAsync();

                Universes.Add(uni);

                if (Universes.Count == 1)
                    await SelectUniverse(uni);
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

        public async Task Vacuum()
        {
            if (Connection != null)
            {
                // Compact the database.
                using (SqliteCommand cmd = new SqliteCommand("VACUUM;", Connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

    }
}
