using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("NoteCategoryDocument")]
    public class NoteCategoryDocument : IDbType, INotifyPropertyChanged
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
        private long _noteDocId;
        private long _noteCatDocId;
        #endregion

        #region Database Properties
        [DbField]
        public long id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("id"); }
        }
        [DbField]
        public long NoteDocumentId
        {
            get { return _noteDocId; }
            set { _noteDocId = value; OnPropertyChanged("NoteDocumentId"); }
        }
        [DbField]
        public long NoteCategoryId
        {
            get { return _noteCatDocId; }
            set { _noteCatDocId = value; OnPropertyChanged("NoteCategoryId"); }
        }
        #endregion

        public SqliteConnection Connection { get; set; }
        private static DbHelper<NoteCategoryDocument> _dbHelper;

        public NoteCategoryDocument(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<NoteCategoryDocument>(Connection);
        }
        public async Task CreateAsync()
        {
            await _dbHelper.InsertAsync(this).ConfigureAwait(false);
            await LoadAsync();
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

        public static async Task<List<NoteCategoryDocument>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<NoteCategoryDocument>(connection);

            List<NoteCategoryDocument> retval = new List<NoteCategoryDocument>();
            List<long> ids = await _dbHelper.GetAllIdsAsync().ConfigureAwait(false);
            foreach (long id in ids)
            {
                NoteCategoryDocument doc = new NoteCategoryDocument(connection);
                doc.id = id;
                await doc.LoadAsync().ConfigureAwait(false);
                retval.Add(doc);
            }
            return retval;
        }

        public static async Task<List<NoteCategoryDocument>> GetCategoriesForDocument(SqliteConnection connection, long docId)
        {
            List<NoteCategoryDocument> items = await LoadAll(connection);
            items = items.Where(i => i.NoteDocumentId == docId).ToList();
            return items;
        }
    }
}
