﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("MarkdownCategoryDocument")]
    public class MarkdownCategoryDocument : IDbType, INotifyPropertyChanged
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
        private long _markdownCategoryId;
        private long _markdownDocumentId;
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
        public long MarkdownCategoryId
        {
            get { return _markdownCategoryId; }
            set
            {
                _markdownCategoryId = value;
                OnPropertyChanged("MarkdownCategoryId");
            }
        }
        [DbField]
        public long MarkdownDocumentId
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
        public SqliteConnection Connection { get; set; }
        private static DbHelper<MarkdownCategoryDocument> _dbHelper;
        #endregion

        public MarkdownCategoryDocument(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<MarkdownCategoryDocument>(connection);
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

        public static List<MarkdownCategoryDocument> GetAllMarkdownDocuments(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<MarkdownCategoryDocument>(connection);

            List<MarkdownCategoryDocument> items = new List<MarkdownCategoryDocument>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (var id in ids)
            {
                MarkdownCategoryDocument item = new MarkdownCategoryDocument(connection);
                item.id = id;
                item.Load();
                items.Add(item);
            }
            return items;
        }

        public static List<MarkdownCategoryDocument> GetCategoriesForDocument(SqliteConnection connection, long docId)
        {
            List<MarkdownCategoryDocument> items = GetAllMarkdownDocuments(connection).Where(i => i.MarkdownDocumentId == docId).ToList();
            return items;
        }
    }
}