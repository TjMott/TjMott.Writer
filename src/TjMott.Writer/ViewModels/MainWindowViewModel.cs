using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public OpenWindowsViewModel OpenWindowsViewModel { get; private set; }
        #region Commands
        public ReactiveCommand<Window, Unit> NewFileCommand { get; }
        public ReactiveCommand<Window, Unit> OpenFileCommand { get; }
        public ReactiveCommand<Unit, Unit> QuitCommand { get; }
        public ReactiveCommand<Window, Unit> AboutCommand { get; }
        #endregion

        private Database _database;
        public Database Database { get => _database; set => this.RaiseAndSetIfChanged(ref _database, value); }

        public MainWindowViewModel(Window owner)
        {
            OpenWindowsViewModel = new OpenWindowsViewModel(owner);
            NewFileCommand = ReactiveCommand.Create<Window>(NewFile);
            OpenFileCommand = ReactiveCommand.Create<Window>(OpenFile);
            QuitCommand = ReactiveCommand.Create(Quit);
            AboutCommand = ReactiveCommand.Create<Window>(ShowAbout);
            initialize(owner);
        }

        private async void initialize(Window owner)
        {
            // Delay to give main window a chance to show. ShowDialog below fails if
            // MainWindow isn't open yet.
            while (!owner.IsActive)
                await Task.Delay(100);

            // Check that CEF initialized.
            if (!CefNetAppImpl.InitSuccess)
            {
                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("CEF Initialization Error",
                    CefNetAppImpl.InitErrorMessage + Environment.NewLine + "You can try to continue using the application, but you will not be able to open Documents.",
                    MessageBox.Avalonia.Enums.ButtonEnum.OkAbort,
                    MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation.CenterOwner);
                var result = await msgbox.ShowDialog(owner);
                if (result == MessageBox.Avalonia.Enums.ButtonResult.Abort)
                {
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(AppSettings.Default.lastFile) && File.Exists(AppSettings.Default.lastFile))
            {
                OpenDatabase(AppSettings.Default.lastFile, owner);
            }
        }

        public async void NewFile(Window owner)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filters.Add(FileDialogFilters.DatabaseFilter);

            string path = await dialog.ShowAsync(owner);
            if (path != null)
            {
                Database = new Database(path);
                await Database.LoadAsync().ConfigureAwait(false);
                AppSettings.Default.lastFile = Database.FileName;
                AppSettings.Default.Save();
            }
        }

        public async void OpenFile(Window owner)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(FileDialogFilters.DatabaseFilter);
            dialog.AllowMultiple = false;

            string[] paths = await dialog.ShowAsync(owner);

            if (paths != null && paths.Length == 1 && File.Exists(paths[0]))
            {
                OpenDatabase(paths[0], owner);
            }
        }

        public async void OpenDatabase(string filename, Window owner)
        {
            Database = new Database(filename);

            if (Database.Metadata.DbVersion > Metadata.ExpectedVersion)
            {
                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Cannot Open File",
                    string.Format("Cannot open file '{0}'. Reason: file is version {1} but this application requires version {2}. The file will now be closed to prevent corruption.", Path.GetFileName(filename), Database.Metadata.DbVersion, Metadata.ExpectedVersion),
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation.CenterOwner);
                await msgbox.ShowDialog(owner);
                Database.Close();
                Database = null;
                return;
            }

            if (Database.RequiresUpgrade)
            {
                DatabaseUpgradeView upgradeView = new DatabaseUpgradeView();
                upgradeView.DataContext = new DatabaseUpgradeViewModel(Database);
                // Delay to give main window a chance to show. ShowDialog below fails if
                // MainWindow isn't open yet.
                while (!owner.IsActive)
                    await Task.Delay(100);
                bool result = await upgradeView.ShowDialog<bool>(owner);
                if (!result)
                {
                    // Upgrade error.
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
                    return;
                }
            }
            await Database.LoadAsync().ConfigureAwait(false);
            AppSettings.Default.lastFile = filename;
            AppSettings.Default.Save();
        }

        public void ShowAbout(Window owner)
        {
            AboutWindow wnd = new AboutWindow();
            wnd.ShowDialog(owner);
        }

        public void Quit()
        {
            if (Database != null)
                Database.Close();
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }
    }
}
