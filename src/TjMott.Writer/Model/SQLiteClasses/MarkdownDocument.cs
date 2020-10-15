using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using TjMott.Writer.Model.Attributes;

namespace TjMott.Writer.Model.SQLiteClasses
{
    [DbTableName("MarkdownDocument")]
    public class MarkdownDocument : IDbType, INotifyPropertyChanged, IHasNameProperty
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
        private string _name;
        private string _markdownText;
        private string _plainText;
        private bool _isSpecial;
        #endregion

        #region Database Properties
        [DbField]
        public long id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("id");
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
        public string MarkdownText
        {
            get { return _markdownText; }
            set
            {
                _markdownText = value;
                OnPropertyChanged("MarkdownText");
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
        public bool IsSpecial
        {
            get { return _isSpecial; }
            set
            {
                _isSpecial = value;
                OnPropertyChanged("IsSpecial");
            }
        }
        #endregion

        #region Properties
        public SQLiteConnection Connection { get; set; }
        #endregion

        private static DbHelper<MarkdownDocument> _dbHelper;
        public MarkdownDocument(SQLiteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<MarkdownDocument>(connection);
            Connection = connection;
            IsSpecial = false;
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

        public static List<MarkdownDocument> GetAllMarkdownDocuments(SQLiteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<MarkdownDocument>(connection);

            List<MarkdownDocument> items = new List<MarkdownDocument>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (var id in ids)
            {
                MarkdownDocument doc = new MarkdownDocument(connection);
                doc.id = id;
                doc.Load();
                items.Add(doc);
            }
            return items;
        }
    }
}
