using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.Sqlite;
using System.Text;
using TjMott.Writer.Models.Attributes;
using System.Threading.Tasks;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("Chapter")]
    public class Chapter : IDbType, INotifyPropertyChanged, ISortable, IHasNameProperty
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
        private long _storyId;
        private long _sortIndex;
        private string _name;
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
        public long StoryId
        {
            get { return _storyId; }
            set
            {
                _storyId = value;
                OnPropertyChanged("StoryId");
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
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        #endregion

        #region Properties
        public SqliteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Chapter> _dbHelper;
        private static DbCommandHelper _chapterLoader;

        public Chapter(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Chapter>(Connection);

            Name = "New Chapter";
            SortIndex = 0;
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

        public Chapter Clone()
        {
            Chapter chap = new Chapter(Connection);
            chap._name = _name;
            chap._sortIndex = _sortIndex;
            return chap;
        }

        public static async Task<List<Chapter>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Chapter>(connection);

            List<Chapter> retval = new List<Chapter>();
            List<long> ids = await _dbHelper.GetAllIdsAsync();
            foreach (long id in ids)
            {
                Chapter chapter = new Chapter(connection);
                chapter.id = id;
                await chapter.LoadAsync();
                retval.Add(chapter);
            }
            return retval;
        }

        public static async Task<List<Chapter>> LoadForStory(long storyId, SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Chapter>(connection);

            if (_chapterLoader == null)
            {
                _chapterLoader = new DbCommandHelper(connection);
                _chapterLoader.Command.CommandText = string.Format("SELECT [id] FROM {0} WHERE {1} = @storyId",
                                                                    _dbHelper.TableName,
                                                                    nameof(StoryId));
                _chapterLoader.AddParameter("@storyId");
            }

            _chapterLoader.Parameters["@storyId"].Value = storyId;
            List<long> ids = new List<long>();
            using (var reader = await _chapterLoader.Command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ids.Add(reader.GetInt64(0));
                }
            }

            List<Chapter> retval = new List<Chapter>();
            foreach (long id in ids)
            {
                Chapter chapter = new Chapter(connection);
                chapter.id = id;
                await chapter.LoadAsync();
                retval.Add(chapter);
            }
            return retval;
        }
    }
}
