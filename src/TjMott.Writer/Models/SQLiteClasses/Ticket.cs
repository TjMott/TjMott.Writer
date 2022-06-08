using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TjMott.Writer.Models.Attributes;

namespace TjMott.Writer.Models.SQLiteClasses
{
    [DbTableName("Ticket")]
    public class Ticket : IDbType, INotifyPropertyChanged, IHasNameProperty
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
        private string _name;
        private string _status;
        private string _dueDate;
        private int _priority;
        private long _universeId;
        private long _documentId;
        #endregion

        #region Database properties
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
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        [DbField]
        public string DueDate
        {
            get { return _dueDate; }
            set
            {
                _dueDate = value;
                OnPropertyChanged("DueDate");
            }
        }

        [DbField]
        public int Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                OnPropertyChanged("Priority");
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
        public long DocumentId
        {
            get { return _documentId; }
            set
            {
                _documentId = value;
                OnPropertyChanged("DocumentId");
            }
        }
        #endregion

        #region Properties
        public SqliteConnection Connection { get; set; }
        #endregion

        private static DbHelper<Ticket> _dbHelper;

        public Ticket(SqliteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Ticket>(Connection);
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

        public static async Task<List<Ticket>> LoadAll(SqliteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<Ticket>(connection);

            List<Ticket> retval = new List<Ticket>();
            List<long> ids = await _dbHelper.GetAllIdsAsync().ConfigureAwait(false);
            foreach (long id in ids)
            {
                Ticket ticket = new Ticket(connection);
                ticket.id = id;
                await ticket.LoadAsync().ConfigureAwait(false);
                retval.Add(ticket);
            }
            return retval;
        }
    }
}
