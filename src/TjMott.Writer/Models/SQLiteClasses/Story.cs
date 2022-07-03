using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("Story")]
    public class Story : IDbType, INotifyPropertyChanged, ISortable, IHasNameProperty
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
        private long _sortIndex;
        private long _universeId;
        private long? _categoryId;
        private long? _copyrightPageId;
        private string _name;
        private string _subtitle;
        private string _author;
        private string _edition;
        private string _isbn;
        private string _asin;
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
        public long? CategoryId
        {
            get { return _categoryId; }
            set
            {
                _categoryId = value;
                OnPropertyChanged("CategoryId");
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
        public string Subtitle
        {
            get { return _subtitle; }
            set
            {
                _subtitle = value;
                OnPropertyChanged("Subtitle");
            }
        }

        [DbField]
        public string Author
        {
            get { return _author; }
            set
            {
                _author = value;
                OnPropertyChanged("Author");
            }
        }

        [DbField]
        public string Edition
        {
            get { return _edition; }
            set
            {
                _edition = value;
                OnPropertyChanged("Edition");
            }
        }

       
        [DbField]
        public string ISBN
        {
            get { return _isbn; }
            set
            {
                _isbn = value;
                OnPropertyChanged("ISBN");
            }
        }

        [DbField]
        public string ASIN
        {
            get { return _asin; }
            set
            {
                _asin = value;
                OnPropertyChanged("ASIN");
            }
        }

        [DbField]
        public long? CopyrightPageId
        {
            get { return _copyrightPageId; }
            set
            {
                _copyrightPageId = value;
                OnPropertyChanged("CopyrightPageId");
            }
        }
        #endregion

        #region Properties
        public SqliteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Story> _dbHelper;

        public Story(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Story>(Connection);

            id = -1;
            Name = "New Story";
            Subtitle = "";
            Author = "";
            Edition = "";
            ISBN = "";
            ASIN = "";
            SortIndex = 0;
        }
        public async Task LoadAsync()
        {
            await _dbHelper.LoadAsync(this).ConfigureAwait(false);
        }

        public async Task CreateAsync()
        {
            await _dbHelper.InsertAsync(this).ConfigureAwait(false);
            await LoadAsync().ConfigureAwait(false);
        }

        public async Task SaveAsync()
        {
            await _dbHelper.UpdateAsync(this).ConfigureAwait(false);
        }

        public async Task DeleteAsync()
        {
            await _dbHelper.DeleteAsync(this).ConfigureAwait(false);
        }

        public Story Clone()
        {
            Story story = new Story(Connection);
            story._name = _name;
            story._subtitle = _subtitle;
            story._author = _author;
            story._edition = _edition;
            story._sortIndex = _sortIndex;
            return story;
        }

        public static async Task<List<Story>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Story>(connection);

            List<Story> retval = new List<Story>();
            List<long> ids = await _dbHelper.GetAllIdsAsync().ConfigureAwait(false);
            foreach (long id in ids)
            {
                Story story = new Story(connection);
                story.id = id;
                await story.LoadAsync().ConfigureAwait(false);
                retval.Add(story);
            }
            return retval;
        }
    }
}
