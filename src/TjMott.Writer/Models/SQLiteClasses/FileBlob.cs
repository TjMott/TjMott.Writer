using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.Sqlite;
using System.IO;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("File")]
    public class FileBlob : IDbType, INotifyPropertyChanged, ISortable, IHasNameProperty
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
        private string _fileName;
        private string _fileType;
        private long _sortIndex;
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
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }
        [DbField]
        public string FileType
        {
            get { return _fileType; }
            set
            {
                _fileType = value;
                OnPropertyChanged("FileType");
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
        #endregion

        #region Properties
        public SqliteConnection Connection { get; set; }
        #endregion

        #region Database helpers
        private static DbHelper<FileBlob> _dbHelper;
        private static DbCommandHelper _selectBlobCmd;
        private static DbCommandHelper _zeroBlobCmd;

        private static void initSql(SqliteConnection con)
        {
            if (_dbHelper == null)
            {
                _dbHelper = new DbHelper<FileBlob>(con);

                _selectBlobCmd = new DbCommandHelper(con);
                _selectBlobCmd.Command.CommandText = "SELECT [Data], length([Data]) as Length FROM [File] WHERE [id] = @id";
                _selectBlobCmd.AddParameter("@id");

                _zeroBlobCmd = new DbCommandHelper(con);
                _zeroBlobCmd.Command.CommandText = "UPDATE [File] SET [Data] = zeroblob(@blobSize) WHERE [id] = @id";
                _zeroBlobCmd.AddParameter("@id");
                _zeroBlobCmd.AddParameter("@blobSize");
            }
        }
        #endregion

        #region File types enum
        public const string FILE_TYPE_PNG = "PNG";
        public const string FILE_TYPE_JPEG = "JPG";
        public const string FILE_TYPE_TEMPLATE = "TEMPLATE";
        public const string FILE_TYPE_OTHER = "OTHER";
        #endregion

        public FileBlob(SqliteConnection connection)
        {
            Connection = connection;
            initSql(Connection);
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

        public void ExportToFile(string filename)
        {
            _selectBlobCmd.Parameters["@id"].Value = id;
            using (SqliteDataReader reader = _selectBlobCmd.Command.ExecuteReader(System.Data.CommandBehavior.KeyInfo))
            {
                if (reader.Read())
                {
                    /*using (SqliteBlob blob = reader.GetBlob(0, true))
                    {
                        int length = reader.GetInt32(1);
                        using (FileStream fs = File.Open(filename, FileMode.Create))
                        {
                            int position = 0;
                            byte[] buf = new byte[1024 * 1024]; // 1 MB buffer
                            while (position < length)
                            {
                                int bytesRead = Math.Min(buf.Length, length - position);
                                blob.Read(buf, bytesRead, position);
                                fs.Write(buf, 0, bytesRead);
                                position += bytesRead;
                            }
                        }
                    }*/
                }
            }
        }

        public void LoadFile(string filename)
        {
            using (FileStream fs = File.Open(filename, FileMode.Open))
            {
                // Init blob.
                _zeroBlobCmd.Parameters["@id"].Value = id;
                _zeroBlobCmd.Parameters["@blobSize"].Value = fs.Length;
                _zeroBlobCmd.Command.ExecuteNonQuery();

                // Write file to blob.
                _selectBlobCmd.Parameters["@id"].Value = id;
                using (SqliteDataReader reader = _selectBlobCmd.Command.ExecuteReader(System.Data.CommandBehavior.KeyInfo))
                {
                    if (reader.Read())
                    {
                        /*using (SqliteBlob blob = reader.GetBlob(0, false))
                        {
                            long length = fs.Length;
                            int position = 0;
                            byte[] buf = new byte[1024 * 1024]; // 1 MB buffer
                            while (position < length)
                            {
                                int bytesRead = (int)Math.Min(buf.Length, length - position);
                                bytesRead = fs.Read(buf, 0, bytesRead);
                                blob.Write(buf, bytesRead, position);
                                position += bytesRead;
                            }
                        }*/
                    }
                }
            }
        }

        public string Base64Encode()
        {
            _selectBlobCmd.Parameters["@id"].Value = id;
            using (SqliteDataReader reader = _selectBlobCmd.Command.ExecuteReader(System.Data.CommandBehavior.KeyInfo))
            {
                if (reader.Read())
                {
                    /*using (SqliteBlob blob = reader.GetBlob(0, true))
                    {
                        int length = reader.GetInt32(1);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int position = 0;
                            byte[] buf = new byte[1024 * 1024]; // 1 MB buffer
                            while (position < length)
                            {
                                int bytesRead = Math.Min(buf.Length, length - position);
                                blob.Read(buf, bytesRead, position);
                                ms.Write(buf, 0, bytesRead);
                                position += bytesRead;
                            }

                            return Convert.ToBase64String(ms.GetBuffer());
                        }
                    }*/
                }
            }
            return "";
        }

        public static List<FileBlob> GetAllFileBlobs(SqliteConnection connection)
        {
            initSql(connection);

            List<FileBlob> retval = new List<FileBlob>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (long id in ids)
            {
                FileBlob file = new FileBlob(connection);
                file.id = id;
                file.Load();
                retval.Add(file);
            }
            return retval;
        }
    }
}
