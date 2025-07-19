using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private string _decryptedJson;
        private string _plainText;
        private long _wordCount;
        private bool _isEncrypted;
        private string _documentType;
        private bool _isUnlocked;
        private string _cachedPassword;
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
            set { _json = value; OnPropertyChanged("Json"); OnPropertyChanged("PublicJson"); }
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

        public string PublicJson
        {
            get
            {
                if (IsEncrypted)
                {
                    if (IsUnlocked)
                        return _decryptedJson;
                    else
                        return "{ \"ops\": [ { \"insert\": \"This document is password-protected and locked.\" } ] }";
                }
                else
                {
                    return _json;
                }
            }
            set
            {
                if (IsEncrypted)
                {
                    if (IsUnlocked)
                        _decryptedJson = value;
                }
                else
                {
                    _json = value;
                }
                OnPropertyChanged("PublicJson");
            }
        }

        public bool IsUnlocked
        {
            get { return _isUnlocked; }
            private set { _isUnlocked = value; OnPropertyChanged("IsUnlocked"); }
        }

        private static DbHelper<Document> _dbHelper;

        public Document(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Document>(Connection);
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
            IsUnlocked = !IsEncrypted;
        }

        public async Task SaveAsync()
        {
            if (IsEncrypted)
            {
                if (IsUnlocked && _cachedPassword != null)
                {
                    Json = AESHelperV2.AesEncrypt(_decryptedJson, _cachedPassword);
                }
                else
                {
                    return;
                }
            }
            await _dbHelper.UpdateAsync(this);
        }

        public static async Task<List<Document>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Document>(connection);

            List<Document> retval = new List<Document>();
            List<long> ids = await _dbHelper.GetAllIdsAsync();
            foreach (long id in ids)
            {
                Document doc = new Document(connection);
                doc.id = id;
                await doc.LoadAsync();
                retval.Add(doc);
            }
            return retval;
        }

        public void Unlock(string password)
        {
            if (!IsEncrypted) return;
            if (IsUnlocked) return;
            string decrypted = AESHelperV2.AesDecrypt(_json, password);
            _cachedPassword = password;
            IsUnlocked = true;
            PublicJson = decrypted;
        }

        public void Lock()
        {
            if (!IsEncrypted) return;
            if (!IsUnlocked) return;
            _decryptedJson = "";
            _cachedPassword = "";
            IsUnlocked = false;
        }

        public async Task Encrypt(string password)
        {
            if (IsEncrypted) return;
            string encrypted = AESHelperV2.AesEncrypt(PublicJson, password);
            _decryptedJson = PublicJson;
            _cachedPassword = password;
            _json = encrypted;
            IsEncrypted = true;
            await SaveAsync();
        }

        public async Task Decrypt(string password)
        {
            if (!IsEncrypted) return;
            string decrypted = AESHelperV2.AesDecrypt(_json, password);
            IsEncrypted = false;
            IsUnlocked = true;
            PublicJson = decrypted;
            await SaveAsync();
        }

        public static void GetNewDocumentContent(out string json, out string plainText)
        {
            json = "{ \"ops\": [ { \"insert\": \"This is a new document.\" } ] }";
            plainText = "This is a new document";
        }

        public JObject GetJObject()
        {
            return JObject.Parse(PublicJson);
        }
    }
}
