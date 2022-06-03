using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.ComponentModel;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    public class NoteDocument : IDbType, INotifyPropertyChanged, IHasNameProperty
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
        private long _documentId;
        private string _name;
        #endregion

        #region Database Properties
        [DbField]
        public long id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("id"); }
        }
        [DbField]
        public long UniverseId
        {
            get { return _universeId; }
            set { _universeId = value; OnPropertyChanged("UniverseId"); }
        }
        [DbField]
        public long DocumentId
        {
            get { return _documentId; }
            set { _documentId = value; OnPropertyChanged("DocumentId"); }
        }
        [DbField]
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }
        #endregion

        public SqliteConnection Connection { get; set; }
        private static DbHelper<NoteDocument> _dbHelper;

        public NoteDocument(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<NoteDocument>(Connection);
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

        public static List<NoteDocument> GetAllDocuments(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<NoteDocument>(connection);

            List<NoteDocument> retval = new List<NoteDocument>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (long id in ids)
            {
                NoteDocument doc = new NoteDocument(connection);
                doc.id = id;
                doc.Load();
                retval.Add(doc);
            }
            return retval;
        }
    }

}
