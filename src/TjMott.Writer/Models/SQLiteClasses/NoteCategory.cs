﻿using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("NoteCategory")]
    public class NoteCategory : IDbType, INotifyPropertyChanged, IHasNameProperty
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
        private long? _parentId;
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
        public long? ParentId
        {
            get { return _parentId; }
            set { _parentId = value; OnPropertyChanged("ParentId"); }
        }
        [DbField]
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged("Name"); }
        }
        #endregion

        public SqliteConnection Connection { get; set; }
        private static DbHelper<NoteCategory> _dbHelper;

        public NoteCategory(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<NoteCategory>(Connection);
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

        public static async Task<List<NoteCategory>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<NoteCategory>(connection);

            List<NoteCategory> retval = new List<NoteCategory>();
            List<long> ids = await _dbHelper.GetAllIdsAsync().ConfigureAwait(false);
            foreach (long id in ids)
            {
                NoteCategory doc = new NoteCategory(connection);
                doc.id = id;
                await doc.LoadAsync().ConfigureAwait(false);
                retval.Add(doc);
            }
            return retval;
        }
    }
}
