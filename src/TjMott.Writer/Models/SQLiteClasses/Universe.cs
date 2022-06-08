using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("Universe")]
    public class Universe : IDbType, INotifyPropertyChanged, ISortable, IHasNameProperty
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
        private long _sortIndex;
        private string _name;
        private long? _defaultTemplateId;
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
        public long? DefaultTemplateId
        {
            get { return _defaultTemplateId; }
            set
            {
                _defaultTemplateId = value;
                OnPropertyChanged("DefaultTemplateId");
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
        #endregion

        #region Properties
        public SqliteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Universe> _dbHelper;

        public Universe(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Universe>(Connection);

            id = -1;
            SortIndex = 0;
            Name = "New Universe";
        }

        public void Create()
        {
            CreateAsync().Wait();
        }

        public void Save()
        {
            SaveAsync().Wait();
        }

        public void Delete()
        {
            DeleteAsync().Wait();
        }

        public void Load()
        {
            LoadAsync().Wait();
        }

        public async Task CreateAsync()
        {
            await _dbHelper.InsertAsync(this).ConfigureAwait(false);
            await LoadAsync().ConfigureAwait(false);
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

        public static async Task<List<Universe>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Universe>(connection);

            List<Universe> retval = new List<Universe>();
            List<long> ids = await _dbHelper.GetAllIdsAsync().ConfigureAwait(false);
            foreach (long id in ids)
            {
                Universe universe = new Universe(connection);
                universe.id = id;
                await universe.LoadAsync().ConfigureAwait(false);
                retval.Add(universe);
            }
            return retval;
        }
    }
}
