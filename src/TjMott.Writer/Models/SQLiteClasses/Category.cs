using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("Category")]
    public class Category : IDbType, INotifyPropertyChanged, ISortable, IHasNameProperty
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Private variables
        private long _id;
        private string _name;
        private long _sortIndex;
        private long _universeId;
        #endregion

        #region Database Properties
        [DbField]
        public long id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }
        
        [DbField]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        
        [DbField]
        public long SortIndex
        {
            get { return _sortIndex; }
            set
            {
                _sortIndex = value;
                OnPropertyChanged("SortIndex");
            }
        }
        
        [DbField]
        public long UniverseId
        {
            get { return _universeId; }
            set
            {
                _universeId = value;
                OnPropertyChanged("UniverseId");
            }
        }
        #endregion

        #region Properties
        public SqliteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Category> _dbHelper;
        private static DbCommandHelper _categoryLoader;

        public Category(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Category>(Connection);

            id = -1;
            Name = "New Series";
            SortIndex = 0;
        }


        public async Task CreateAsync()
        {
            await _dbHelper.InsertAsync(this).ConfigureAwait(false);
            await LoadAsync();
        }

        public async Task DeleteAsync()
        {
            await _dbHelper.DeleteAsync(this).ConfigureAwait(false);
        }

        public async Task LoadAsync()
        {
            await _dbHelper.LoadAsync(this).ConfigureAwait(false);
        }

        public async Task SaveAsync()
        {
            await _dbHelper.UpdateAsync(this).ConfigureAwait(false);
        }

        public static async Task<List<Category>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Category>(connection);

            List<Category> retval = new List<Category>();
            List<long> ids = await _dbHelper.GetAllIdsAsync().ConfigureAwait(false);
            foreach (long id in ids)
            {
                Category series = new Category(connection);
                series.id = id;
                await series.LoadAsync().ConfigureAwait(false);
                retval.Add(series);
            }
            return retval;
        }

        public static async Task<List<Category>> LoadForUniverse(long universeId, SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Category>(connection);

            if (_categoryLoader == null)
            {
                _categoryLoader = new DbCommandHelper(connection);
                _categoryLoader.Command.CommandText = string.Format("SELECT [id] FROM {0} WHERE {1} = @universeId",
                                                                    _dbHelper.TableName,
                                                                    nameof(UniverseId));
                _categoryLoader.AddParameter("@universeId");
            }

            _categoryLoader.Parameters["@universeId"].Value = universeId;
            List<long> ids = new List<long>();
            using (var reader = await _categoryLoader.Command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ids.Add(reader.GetInt64(0));
                }
            }

            List<Category> retval = new List<Category>();
            foreach (long id in ids)
            {
                Category category = new Category(connection);
                category.id = id;
                await category.LoadAsync().ConfigureAwait(false);
                retval.Add(category);
            }
            return retval;
        }
    }
}
