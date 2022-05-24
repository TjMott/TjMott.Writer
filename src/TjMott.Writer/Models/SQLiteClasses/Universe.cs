using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            _dbHelper.Insert(this);
            Load();
        }

        public void Delete()
        {
            _dbHelper.Delete(this);
        }

        public void Load()
        {
            _dbHelper.Load(this);
        }

        public void Save()
        {
            _dbHelper.Update(this);
        }

        public static List<Universe> GetAllUniverses(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Universe>(connection);

            List<Universe> retval = new List<Universe>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (long id in ids)
            {
                Universe universe = new Universe(connection);
                universe.id = id;
                universe.Load();
                retval.Add(universe);
            }
            return retval;
        }
    }
}
