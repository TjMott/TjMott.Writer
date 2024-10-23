using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CefNet;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using TjMott.Writer.Controls;
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
        public ReactiveCommand<Window, Unit> QuitCommand { get; }
        public ReactiveCommand<Window, Unit> AboutCommand { get; }
        #endregion

        private Database _database;
        public Database Database { get => _database; set => this.RaiseAndSetIfChanged(ref _database, value); }

        public class UniverseMenuItem : ReactiveObject
        {
            private bool _isChecked;
            public bool IsChecked { get => _isChecked; set => this.RaiseAndSetIfChanged(ref _isChecked, value); }
            private Universe _universe;
            public Universe Universe {  get => _universe; set => this.RaiseAndSetIfChanged(ref _universe, value); }
            public ReactiveCommand<Universe, Unit> Command { get; set; }
        }

        public ObservableCollection<UniverseMenuItem> UniverseMenuItems { get; private set; }

        public MainWindowViewModel(Window owner)
        {
            UniverseMenuItems = new ObservableCollection<UniverseMenuItem>();
            OpenWindowsViewModel = new OpenWindowsViewModel(owner);
            NewFileCommand = ReactiveCommand.Create<Window>(NewFile);
            OpenFileCommand = ReactiveCommand.Create<Window>(OpenFile);
            QuitCommand = ReactiveCommand.Create<Window>(Quit);
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
                if (!CefNetAppImpl.IsCefInstalled)
                {
                    var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("CEF Not Installed",
                        "CEF, a necessary component for the editor, is not installed." + Environment.NewLine +
                        "Would you like to install it now?" + Environment.NewLine +
                        "You can continue using the application without it," + Environment.NewLine + "but you will not be able to open or view any documents.",
                        MessageBox.Avalonia.Enums.ButtonEnum.YesNoAbort,
                        MessageBox.Avalonia.Enums.Icon.Question,
                        WindowStartupLocation.CenterOwner);
                    var result = await msgbox.ShowDialog(owner);
                    if (result == MessageBox.Avalonia.Enums.ButtonResult.Abort)
                    {
                        (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
                        return;
                    }
                    else if (result == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                    {
                        CefNetAppImpl.RestartAndInstallCef();
                        return;
                    }
                }
                else
                {
                    var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("CEF Initialization Error",
                        CefNetAppImpl.InitErrorMessage + Environment.NewLine + "You can try to continue using the application," + Environment.NewLine +  "but you will not be able to open or view any documents.",
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
            }

            // Check QuillJS hashes.
            // Was nice in theory, but somehow the hashing algorithm is sensitive to newline differences between Windows and Linux even though it operates in byte mode.
            // Disable for now.
            /*bool quillVerified = await QuillJsEditor.VerifyHashes();
            if (!quillVerified)
            {
                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Editor Initialization Error",
                       "Editor assets failed SHA256 hash verification. Your installation" + Environment.NewLine +
                       "may have been corrupted by malware. You can continue to use the" + Environment.NewLine + 
                       "application, but you may face data corruption or malware issues." + Environment.NewLine +
                       "It is recommended you reinstall first.",
                    MessageBox.Avalonia.Enums.ButtonEnum.OkAbort,
                    MessageBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation.CenterOwner);
                var result = await msgbox.ShowDialog(owner);
                if (result == MessageBox.Avalonia.Enums.ButtonResult.Abort)
                {
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
                    return;
                }
            }*/

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
            loadUniversesMenu();
            Database.Universes.CollectionChanged += (o, e) => loadUniversesMenu();
        }

        public async void ShowAbout(Window owner)
        {
            await new AboutWindow().ShowDialog(owner);
        }

        public async void Quit(Window dialogOwner)
        {
            if (OpenWindowsViewModel.AllWindows.Count() > 1)
            {
                await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Open Windows",
                    "You have several open windows. Please save and close your work before closing the main window.",
                    MessageBox.Avalonia.Enums.ButtonEnum.Ok,
                    MessageBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation.CenterOwner).ShowDialog(dialogOwner);
                return;
            }
            if (Database != null)
                Database.Close();
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }

        public async void ShowQuillHashes(Window owner)
        {
            await new QuillHashWindow().ShowDialog(owner);
        }

        public void ShowReadmeWindow()
        {
            ReadmeWindow.ShowReadmeWindow();
        }

        public void ShowWordTemplates()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = Path.Combine(Directory.GetCurrentDirectory(), "WordTemplates");
            Process.Start(psi);
        }

        public void SelectUniverse(Universe u)
        {
            foreach (var item in UniverseMenuItems)
            {
                item.IsChecked = item.Universe == u;
            }
            _ = Database.SelectUniverse(u);
        }

        private void loadUniversesMenu()
        {
            UniverseMenuItems.Clear();
            if (Database != null)
            {
                foreach (var u in Database.Universes)
                {
                    UniverseMenuItem menuItem = new UniverseMenuItem();
                    menuItem.Universe = u;
                    menuItem.IsChecked = Database.SelectedUniverse == u;
                    menuItem.Command = ReactiveCommand.Create<Universe>(SelectUniverse);
                    UniverseMenuItems.Add(menuItem);
                }
            }
        }

        public async void InstallCef(Window dialogOwner)
        {
           var buttonResult = await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Install CEF?",
                                    "Install CEF? This will restart the application.",
                                    MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                                    MessageBox.Avalonia.Enums.Icon.Question,
                                    WindowStartupLocation.CenterOwner).ShowDialog(dialogOwner);

            if (buttonResult == MessageBox.Avalonia.Enums.ButtonResult.Yes)
            {
                if (Database != null)
                    Database.Close();
                CefNetAppImpl.RestartAndInstallCef();
            }
        }
    }
}
