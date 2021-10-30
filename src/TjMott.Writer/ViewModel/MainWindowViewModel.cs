using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Properties;
using TjMott.Writer.Windows;

namespace TjMott.Writer.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Private variables
        private MainWindow _mainWindow;
        #endregion

        #region Properties
        private Database _database;
        public Database Database
        {
            get { return _database; }
            private set
            {
                _database = value;
                OnPropertyChanged("Database");
                OnPropertyChanged("UniverseMenuEnabled");
            }
        }

        public bool UniverseMenuEnabled
        {
            get { return _database != null; }
        }
        #endregion

        #region Commands
        private ICommand _createDatabaseCommand;
        public ICommand CreateDatabaseCommand
        {
            get
            {
                if (_createDatabaseCommand == null)
                {
                    _createDatabaseCommand = new RelayCommand(param => CreateNewDatabase());
                }
                return _createDatabaseCommand;
            }
        }

        private ICommand _openDatabaseCommand;
        public ICommand OpenDatabaseCommand
        {
            get
            {
                if (_openDatabaseCommand == null)
                {
                    _openDatabaseCommand = new RelayCommand(param => OpenDatabase());
                }
                return _openDatabaseCommand;
            }
        }

        // Only used to enable/disable the submenu.
        private ICommand _selectUniverseCommand;
        public ICommand SelectUniverseCommand
        {
            get
            {
                if (_selectUniverseCommand == null)
                {
                    _selectUniverseCommand = new RelayCommand(param => { }, param => CanSelectUniverse());
                }
                return _selectUniverseCommand;
            }
        }

        private ICommand _quitCommand;
        public ICommand QuitCommand
        {
            get
            {
                if (_quitCommand == null)
                {
                    _quitCommand = new RelayCommand(param => Exit());
                }
                return _quitCommand;
            }
        }
        #endregion

        public MainWindowViewModel(MainWindow window)
        {
            _mainWindow = window;
        }

        public void CreateNewDatabase()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Writer Database (*.wdb)|*.wdb";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Database = new Database(dialog.FileName);
                Database.Load();
                AppSettings.Default.lastFile = Database.FileName;
                AppSettings.Default.Save();
            }
        }

        public void OpenDatabase()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Writer Database (*.wdb)|*.wdb";
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Database = new Database(dialog.FileName);
                AppSettings.Default.lastFile = Database.FileName;
                AppSettings.Default.Save();

                if (Database.RequiresUpgrade)
                {
                    DatabaseUpgradeDialog upgradeDialog = new DatabaseUpgradeDialog();
                    upgradeDialog.Database = Database;
                    upgradeDialog.Owner = _mainWindow;
                    upgradeDialog.ShowDialog();
                }
                else
                {
                    Database.Load();
                }
            }
        }

        public void OpenDatabase(string filename)
        {
            Database = new Database(filename);
            AppSettings.Default.lastFile = Database.FileName;
            AppSettings.Default.Save();
            if (Database.RequiresUpgrade)
            {
                DatabaseUpgradeDialog upgradeDialog = new DatabaseUpgradeDialog();
                upgradeDialog.Database = Database;
                upgradeDialog.Owner = _mainWindow;
                upgradeDialog.ShowDialog();
            }
            else
            {
                Database.Load();
            }
        }


        public bool CanSelectUniverse()
        {
            if (_database == null)
                return false;
            return _database.Universes.Count > 0;
        }

        public void Exit()
        {
            if (_database != null)
                _database.Close();
            Application.Current.Shutdown();
        }
    }

}
