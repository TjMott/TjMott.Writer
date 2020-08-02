using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Text;
using System.Windows.Media;
using TjMott.Writer.Model.Attributes;

namespace TjMott.Writer.Model.SQLiteClasses
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
        private long _flowDocumentId;
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
        public long FlowDocumentId
        {
            get { return _flowDocumentId; }
            set
            {
                _flowDocumentId = value;
                OnPropertyChanged("FlowDocumentId");
            }
        }
        #endregion

        #region Properties
        public SQLiteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Scene> _dbHelper;

        public Scene(SQLiteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Scene>(Connection);

            initSql(Connection);

            id = -1;
            Name = "New Scene";
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

        #region Flow document serialization
        private static DbCommandHelper _updateDocumentCmd;
        private static DbCommandHelper _selectDocumentCmd;

        private static void initSql(SQLiteConnection con)
        {
            if (_updateDocumentCmd == null)
            {
                _updateDocumentCmd = new DbCommandHelper(con);
                _updateDocumentCmd.Command.CommandText = "UPDATE Scene SET [FlowDocumentXml] = @xml, [DocumentRawText] = @raw WHERE [id] = @id";
                _updateDocumentCmd.AddParameter("@xml");
                _updateDocumentCmd.AddParameter("@raw");
                _updateDocumentCmd.AddParameter("@id");

                _selectDocumentCmd = new DbCommandHelper(con);
                _selectDocumentCmd.Command.CommandText = "SELECT [FlowDocumentXml], [DocumentRawText] FROM Scene WHERE [id] = @id";
                _selectDocumentCmd.AddParameter("@id");
            }
        }
        #endregion

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

        public static List<Scene> GetAllScenes(SQLiteConnection connection)
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
