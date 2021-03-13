using System;
using System.ComponentModel;
using System.Data.SQLite;
using TjMott.Writer.Model.Attributes;

namespace TjMott.Writer.Model.SQLiteClasses
{
    [DbTableName("FlowDocument")]
    public class FlowDocument : IDbType, INotifyPropertyChanged
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
        private long _wordCount;
        private string _xml;
        private string _plainText;
        private bool _isEncrypted;
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
        public long WordCount
        {
            get { return _wordCount; }
            set
            {
                _wordCount = value;
                OnPropertyChanged("WordCount");
            }
        }

        [DbField]
        public string Xml
        {
            get { return _xml; }
            set
            {
                _xml = value;
                OnPropertyChanged("Xml");
            }
        }

        [DbField]
        public string PlainText
        {
            get { return _plainText; }
            set
            {
                _plainText = value;
                OnPropertyChanged("PlainText");
            }
        }
        [DbField]
        public bool IsEncrypted
        {
            get { return _isEncrypted; }
            set
            {
                _isEncrypted = value;
                OnPropertyChanged("IsEncrypted");
            }
        }
        #endregion

        #region Properties
        public SQLiteConnection Connection { get; set; }
        #endregion

        private static DbHelper<FlowDocument> _dbHelper;

        public FlowDocument(SQLiteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<FlowDocument>(connection);
            Connection = connection;
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
    }
}
