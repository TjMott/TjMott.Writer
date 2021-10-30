using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TjMott.Writer.Model.Scripts;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class DatabaseUpgradeViewModel : ViewModelBase
    {
        #region Private variables
        private string _file;
        private string _backupFile;
        private Database _db;
        #endregion

        #region Properties
        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                _isBusy = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged("IsBusy");
                });
            }
        }

        private bool _success = false;
        public bool Success
        {
            get { return _success; }
            private set
            {
                _success = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged("Success");
                });
            }
        }

        private Exception _error;
        public Exception Error
        {
            get { return _error; }
            private set
            {
                _error = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged("Error");
                });
            }
        }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get { return _statusMessage; }
            private set
            {
                _statusMessage = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged("StatusMessage");
                });
            }
        }

        private Visibility _upgradeButtonVisibility = Visibility.Collapsed;
        public Visibility UpgradeButtonVisibility
        {
            get { return _upgradeButtonVisibility; }
            private set
            {
                _upgradeButtonVisibility = value;
                OnPropertyChanged("UpgradeButtonVisibility");
            }
        }

        private Visibility _quitButtonVisibility = Visibility.Collapsed;
        public Visibility QuitButtonVisibility
        {
            get { return _quitButtonVisibility; }
            private set
            {
                _quitButtonVisibility = value;
                OnPropertyChanged("QuitButtonVisibility");
            }
        }

        private Visibility _okButtonVisibility = Visibility.Collapsed;
        public Visibility OkButtonVisibility
        {
            get { return _okButtonVisibility; }
            private set
            {
                _okButtonVisibility = value;
                OnPropertyChanged("OkButtonVisibility");
            }
        }
        #endregion

        #region Commands
        private ICommand _doUpgradeCommand;
        public ICommand DoUpgradeCommand
        {
            get
            {
                if (_doUpgradeCommand == null)
                {
                    _doUpgradeCommand = new RelayCommand(param => PerformUpgrade());
                }
                return _doUpgradeCommand;
            }
        }
        #endregion

        public DatabaseUpgradeViewModel(Database db)
        {
            _db = db;

            StatusMessage = string.Format("Your works database needs to be upgraded. It is currently version {0} and this version of the application requires {1}. Do you want to upgrade now?\r\n\r\nIf you do not upgrade, the application will be closed.",
                                          _db.Metadata.DbVersion,
                                          Metadata.ExpectedVersion);
            UpgradeButtonVisibility = Visibility.Visible;
            QuitButtonVisibility = Visibility.Visible;
            OkButtonVisibility = Visibility.Collapsed;
        }

        public void PerformUpgrade()
        {
            IsBusy = true;
            UpgradeButtonVisibility = Visibility.Collapsed;
            QuitButtonVisibility = Visibility.Collapsed;

            // Create backup.
            StatusMessage = "Backing up works database.";
            _file = _db.FileName;
            string path = Path.GetDirectoryName(_db.FileName);
            string file = Path.GetFileName(_db.FileName);
            _backupFile = Path.Combine(path, file + "_bak");

            // Delete old backup if it already exists.
            if (File.Exists(_backupFile))
            {
                File.Delete(_backupFile);
            }

            File.Copy(_file, _backupFile);

            // Do upgrade on background thread, to keep UI/message pump responsive
            // if this takes a while.
            ThreadPool.QueueUserWorkItem((o) =>
            {
                bool loop = true;
                try
                {
                    while (loop)
                    {
                        int version = _db.Metadata.DbVersion;
                        upgradeOneVersion();
                        if (!_db.RequiresUpgrade)
                        {
                            // Upgrade complete.
                            loop = false;
                            Success = true;
                        }
                        else if (version == _db.Metadata.DbVersion)
                        {
                            // Something went wrong, db version was not updated.
                            loop = false;
                            Success = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Something went wrong, capture the error
                    // so we can report it to the user.
                    Success = false;
                    Error = ex;
                }
                // Move back to UI thread to finish up.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    onUpgradeCompleted();
                });
            });
        }

        private void upgradeOneVersion()
        {
            int version = _db.Metadata.DbVersion;
            string upgradeScript = "";
            switch (version)
            {
                case 1:
                    upgradeScript = Update1to2.Script;
                    break;
                default:
                    throw new ApplicationException(string.Format("No upgrade script exists for database version {0}.", version));
            }

            using (SQLiteCommand cmd = new SQLiteCommand(_db.Connection))
            {
                cmd.CommandText = upgradeScript;
                int result = cmd.ExecuteNonQuery();
            }
        }

        private void onUpgradeCompleted()
        {
            IsBusy = false;
            if (Success)
            {
                _db.Load();
                StatusMessage = "Works database upgraded successfully!";
                OkButtonVisibility = Visibility.Visible;
            }
            else
            {
                // Close and delete database, restore backup.
                _db.Close();
                File.Delete(_file);
                File.Move(_backupFile, _file);
                StatusMessage = string.Format("An error occurred while upgrading the works database. Your database has not been modified. The application will be closed.\r\nError Type: {0}\r\nMessage: {1}", Error.GetType(), Error.Message);
                QuitButtonVisibility = Visibility.Visible;
            }
        }
    }
}
