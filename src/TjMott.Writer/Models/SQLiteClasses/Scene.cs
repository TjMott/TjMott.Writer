using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.Sqlite;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("Scene")]
    public class Scene : IDbType, INotifyPropertyChanged, ISortable, IHasNameProperty
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
        private long _chapterId;
        private string _name;
        private long _sortIndex;
        private long _documentId;
        private byte _colorA;
        private byte _colorR;
        private byte _colorG;
        private byte _colorB;
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
        public long ChapterId
        {
            get { return _chapterId; }
            set
            {
                _chapterId = value;
                OnPropertyChanged("ChapterId");
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
        public byte ColorA
        {
            get { return _colorA; }
            set
            {
                _colorA = value;
            }
        }
        
        [DbField]
        public byte ColorR
        {
            get { return _colorR; }
            set
            {
                _colorR = value;
            }
        }
        
        [DbField]
        public byte ColorG
        {
            get { return _colorG; }
            set
            {
                _colorG = value;
            }
        }
        
        [DbField]
        public byte ColorB
        {
            get { return _colorB; }
            set
            {
                _colorB = value;
            }
        }

        [DbField]
        public long DocumentId
        {
            get { return _documentId; }
            set
            {
                _documentId = value;
                OnPropertyChanged("DocumentId");
            }
        }
        #endregion

        #region Properties
        public SqliteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Scene> _dbHelper;

        public Scene(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Scene>(Connection);

            id = -1;
            Name = "New Scene";
            SortIndex = 0;
            ColorA = 255;
            ColorR = 0;
            ColorG = 0;
            ColorB = 0;
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

        public Scene Clone()
        {
            Scene clone = new Scene(Connection);
            clone._name = _name;
            clone._colorA = _colorA;
            clone._colorR = _colorR;
            clone._colorG = _colorG;
            clone._colorB = _colorB;
            return clone;
        }

        public static List<Scene> GetAllScenes(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Scene>(connection);

            List<Scene> retval = new List<Scene>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (long id in ids)
            {
                Scene scene = new Scene(connection);
                scene.id = id;
                scene.Load();
                retval.Add(scene);
            }
            return retval;
        }
    }
}
