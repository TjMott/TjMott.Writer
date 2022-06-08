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

        #region Commands
        public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
        public ReactiveCommand<Unit, Unit> QuitCommand { get; }
        public ReactiveCommand<Unit, Unit> AboutCommand { get; }
        #endregion

        private Database _database;
        public Database Database { get => _database; set => this.RaiseAndSetIfChanged(ref _database, value); }

        public MainWindowViewModel()
        {
            NewFileCommand = ReactiveCommand.Create(NewFile);
            OpenFileCommand = ReactiveCommand.Create(OpenFile);
            QuitCommand = ReactiveCommand.Create(Quit);
            AboutCommand = ReactiveCommand.Create(ShowAbout);
            initialize();
        }

        private async void initialize()
        {
            // Delay to give main window a chance to show. ShowDialog below fails if
            // MainWindow isn't open yet.
            while (!MainWindow.IsActive)
                await Task.Delay(100);

            // Check that CEF initialized.
            if (!CefNetAppImpl.InitSuccess)
            {
                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("CEF Initialization Error",
                    CefNetAppImpl.InitErrorMessage + Environment.NewLine + "You can try to continue using the application, but you will not be able to open Documents.",
                    MessageBox.Avalonia.Enums.ButtonEnum.OkAbort,
                    MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation.CenterOwner);
                var result = await msgbox.ShowDialog(MainWindow);
                if (result == MessageBox.Avalonia.Enums.ButtonResult.Abort)
                {
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(AppSettings.Default.lastFile) && File.Exists(AppSettings.Default.lastFile))
            {
                OpenDatabase(AppSettings.Default.lastFile);
            }
        }

        public async void NewFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filters.Add(FileDialogFilters.DatabaseFilter);

            string path = await dialog.ShowAsync(MainWindow);
            if (path != null)
            {
                Database = new Database(path);
                await Database.LoadAsync().ConfigureAwait(false);
                AppSettings.Default.lastFile = Database.FileName;
                AppSettings.Default.Save();
            }
        }

        public async void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(FileDialogFilters.DatabaseFilter);
            dialog.AllowMultiple = false;

            string[] paths = await dialog.ShowAsync(MainWindow);

            if (paths != null && paths.Length == 1 && File.Exists(paths[0]))
            {
                OpenDatabase(paths[0]);
            }
        }

        public async void OpenDatabase(string filename)
        {
            Database = new Database(filename);

            if (Database.Metadata.DbVersion > Metadata.ExpectedVersion)
            {
                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Cannot Open File",
                    string.Format("Cannot open file '{0}'. Reason: file is version {1} but this application requires version {2}. The file will now be closed to prevent corruption.", Path.GetFileName(filename), Database.Metadata.DbVersion, Metadata.ExpectedVersion),
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation.CenterOwner);
                await msgbox.ShowDialog(MainWindow);
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
                while (!MainWindow.IsActive)
                    await Task.Delay(100);
                bool result = await upgradeView.ShowDialog<bool>(MainWindow);
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

        public void ShowAbout()
        {
            AboutWindow wnd = new AboutWindow();
            wnd.ShowDialog(MainWindow);
        }

        public void Quit()
        {
            if (Database != null)
                Database.Close();
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }
    }
}
