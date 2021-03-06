﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Text;
using TjMott.Writer.Model.Attributes;

namespace TjMott.Writer.Model.SQLiteClasses
{
    [DbTableName("Chapter")]
    public class Chapter : IDbType, INotifyPropertyChanged, ISortable, IHasNameProperty, IHasMarkdownDocument
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
        private long? _markdownDocumentId;
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

        [DbField]
        public long? MarkdownDocumentId
        {
            get { return _markdownDocumentId; }
            set
            {
                _markdownDocumentId = value;
                OnPropertyChanged("MarkdownDocumentId");
            }
        }
        #endregion

        #region Properties
        public SQLiteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Chapter> _dbHelper;

        public Chapter(SQLiteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Chapter>(Connection);

            Name = "New Chapter";
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

        public Chapter Clone()
        {
            Chapter chap = new Chapter(Connection);
            chap._name = _name;
            chap._sortIndex = _sortIndex;
            return chap;
        }

        public static List<Chapter> GetAllChapters(SQLiteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Chapter>(connection);

            List<Chapter> retval = new List<Chapter>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (long id in ids)
            {
                Chapter chapter = new Chapter(connection);
                chapter.id = id;
                chapter.Load();
                retval.Add(chapter);
            }
            return retval;
        }
    }
}
