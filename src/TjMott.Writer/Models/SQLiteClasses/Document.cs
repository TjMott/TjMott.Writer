using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("Document")]
    public class Document : IDbType, INotifyPropertyChanged 
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public static IEnumerable<string> DocumentTypes = new List<string>() { "Manuscript", "Note", "Ticket" };

        #region Private variables
        private long _id;
        private long _universeId;
        private string _json;
        private string _plainText;
        private long _wordCount;
        private bool _isEncrypted;
        private string _documentType;
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
        public string Json
        {
            get { return _json; }
            set { _json = value; OnPropertyChanged("Json"); }
        }
        [DbField]
        public string PlainText
        {
            get { return _plainText; }
            set { _plainText = value; OnPropertyChanged("PlainText"); }
        }
        [DbField]
        public long WordCount
        {
            get {  return _wordCount; }
            set { _wordCount = value; OnPropertyChanged("WordCount"); }
        }
        [DbField]
        public bool IsEncrypted
        {
            get { return _isEncrypted; }
            set { _isEncrypted = value; OnPropertyChanged("IsEncrypted"); }
        }
        [DbField]
        public string DocumentType
        {
            get { return _documentType; }
            set { _documentType = value; OnPropertyChanged("DocumentType"); }
        }

        public SqliteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Document> _dbHelper;

        public Document(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Document>(Connection);
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

        public static async Task<List<Document>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Document>(connection);

            List<Document> retval = new List<Document>();
            List<long> ids = await _dbHelper.GetAllIdsAsync().ConfigureAwait(false);
            foreach (long id in ids)
            {
                Document doc = new Document(connection);
                doc.id = id;
                await doc.LoadAsync().ConfigureAwait(false);
                retval.Add(doc);
            }
            return retval;
        }

    }
}
