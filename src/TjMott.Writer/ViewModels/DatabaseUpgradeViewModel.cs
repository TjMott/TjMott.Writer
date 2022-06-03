using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using MessageBox.Avalonia;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Models.SqlScripts;

namespace TjMott.Writer.ViewModels
{
    public class DatabaseUpgradeViewModel : ViewModelBase
    {
        private Database _db;
        private string _statusMessage;
        public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }

        private bool _isBusy = false;
        public bool IsBusy { get => _isBusy; private set => this.RaiseAndSetIfChanged(ref _isBusy, value); }

        private bool _showOkButton = false;
        public bool ShowOkButton { get => _showOkButton; private set => this.RaiseAndSetIfChanged(ref _showOkButton, value); }

        private bool _showUpgradeButton = true;
        public bool ShowUpgradeButton { get => _showUpgradeButton; private set => this.RaiseAndSetIfChanged(ref _showUpgradeButton, value); }

        private bool _showQuitButton = true;
        public bool ShowQuitButton { get => _showQuitButton; private set => this.RaiseAndSetIfChanged(ref _showQuitButton, value); }


        public ReactiveCommand<Window, Unit> DoUpgradeCommand { get; }



        public DatabaseUpgradeViewModel(Database db)
        {
            _db = db;
            StatusMessage = string.Format("Your works database needs to be upgraded. It is currently version {0} and this version of the application requires {1}. Do you want to upgrade now?\r\n\r\nIf you do not upgrade, the application will be closed.",
                                          _db.Metadata.DbVersion,
                                          Metadata.ExpectedVersion);
            DoUpgradeCommand = ReactiveCommand.Create<Window>(doUpgrade);
        }

        private async void doUpgrade(Window wnd)
        {
            IsBusy = true;
            ShowUpgradeButton = false;
            ShowQuitButton = false;

            // Create backup.
            StatusMessage = "Backing up database.";
            string filename = _db.FileName;
            string path = Path.GetDirectoryName(_db.FileName);
            string file = Path.GetFileName(_db.FileName);
            string backupFile = Path.Combine(path, file + "_bak");
            File.Copy(filename, backupFile, true);

            // Disable foreign keys.
            using (var cmd = new SqliteCommand("PRAGMA foreign_keys = OFF;", _db.Connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }
            // Start transaction (has a huge performance improvement during the upgrade)
            using (var cmd = new SqliteCommand("BEGIN TRANSACTION;", _db.Connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }
            StatusMessage = "Performing upgrade.";

            bool loop = true;
            //try
            //{
                while (loop)
                {
                    int version = _db.Metadata.DbVersion;
                    DbUpgrader upgrader = null;

                    switch (version)
                    {
                        case 1:
                            upgrader = new Update1to2();
                            break;
                        case 2:
                            upgrader = new Update2to3();
                            break;
                        case 3:
                            upgrader = new Update3to4();
                            break;
                    }

                    if (upgrader != null)
                    {
                        bool success = await upgrader.DoUpgradeAsync(_db.Connection, wnd);
                        if (!success)
                        {
                            loop = false;
                        }
                    }

                    if (!_db.RequiresUpgrade)
                    {
                        // Upgrade complete.
                        loop = false;
                    }
                    else if (version == _db.Metadata.DbVersion)
                    {
                        // Something went wrong, db version was not updated.
                        loop = false;
                    }
                }

                // Re-enable foreign keys.
                using (var cmd = new SqliteCommand("PRAGMA foreign_keys = ON;", _db.Connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // Commit transaction.
                using (var cmd = new SqliteCommand("COMMIT TRANSACTION;", _db.Connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // Done!
                StatusMessage = "Database upgraded successfully!";
                IsBusy = false;
                ShowOkButton = true;
            /*}
            catch (Exception ex)
            {
                // Roll back transaction.
                using (var cmd = new SqliteCommand("ROLLBACK TRANSACTION;", _db.Connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                var msgBox = MessageBoxManager.GetMessageBoxStandardWindow("Database Upgrade Error",
                    ex.Message,
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation.CenterOwner);
                await msgBox.ShowDialog(wnd);
                ShowQuitButton = true;
                IsBusy = false;
            }*/
        }
    }
}
