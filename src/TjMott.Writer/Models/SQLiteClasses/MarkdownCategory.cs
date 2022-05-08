using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.Sqlite;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("MarkdownCategory")]
    public class MarkdownCategory : IDbType, INotifyPropertyChanged, IHasNameProperty
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
        private long _universeId;
        private long? _parentId;
        private string _name;
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
        public long? ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged("ParentId");
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
        private static DbHelper<MarkdownCategory> _dbHelper;
        #endregion

        public MarkdownCategory(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<MarkdownCategory>(connection);
            Connection = connection;
            ParentId = null;
            id = -1;
        }

        public void Load()
        {
            _dbHelper.Load(this);
        }

        public void Create()
        {
            _dbHelper.Insert(this);
            Load();
        }

        public void Save()
        {
            _dbHelper.Update(this);
        }

        public void Delete()
        {
            _dbHelper.Delete(this);
        }

        public static List<MarkdownCategory> GetAllMarkdownDocuments(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<MarkdownCategory>(connection);

            List<MarkdownCategory> items = new List<MarkdownCategory>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (var id in ids)
            {
                MarkdownCategory item = new MarkdownCategory(connection);
                item.id = id;
                item.Load();
                items.Add(item);
            }
            return items;
        }
    }
}
