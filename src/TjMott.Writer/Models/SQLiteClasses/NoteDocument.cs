using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("NoteDocument")]
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

        public async Task CreateAsync()
        {
            await _dbHelper.InsertAsync(this);
            await LoadAsync();
        }

        public async Task DeleteAsync()
        {
            await _dbHelper.DeleteAsync(this);
        }

        public async Task LoadAsync()
        {
            await _dbHelper.LoadAsync(this);
        }

        public async Task SaveAsync()
        {
            await _dbHelper.UpdateAsync(this);
        }

        public static async Task<List<NoteDocument>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<NoteDocument>(connection);

            List<NoteDocument> retval = new List<NoteDocument>();
            List<long> ids = await _dbHelper.GetAllIdsAsync();
            foreach (long id in ids)
            {
                NoteDocument doc = new NoteDocument(connection);
                doc.id = id;
                await doc.LoadAsync();
                retval.Add(doc);
            }
            return retval;
        }
    }

}
