using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using TjMott.Writer.Model.Attributes;

namespace TjMott.Writer.Model.SQLiteClasses
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
        private long? _markdownDocumentId;
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

        [DbField]
        public long? MarkdownDocumentId
        {
            get { return _markdownDocumentId; }
            set
            {
                _markdownDocumentId = value;
                OnPropertyChanged("MarkdownDocumentId");
            }
        }
        #endregion

        #region Properties
        public SQLiteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Category> _dbHelper;

        public Category(SQLiteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Category>(Connection);

            id = -1;
            Name = "New Series";
            SortIndex = 0;
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

        public static List<Category> GetAllSeries(SQLiteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Category>(connection);

            List<Category> retval = new List<Category>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (long id in ids)
            {
                Category series = new Category(connection);
                series.id = id;
                series.Load();
                retval.Add(series);
            }
            return retval;
        }
    }
}
