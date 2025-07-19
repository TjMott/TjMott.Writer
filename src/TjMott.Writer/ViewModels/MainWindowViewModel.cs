using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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
        public ReactiveCommand<Window, Unit> ShowAboutCommand { get; }
        public ReactiveCommand<Window, Unit> ShowQuillHashesCommand { get; }
        public ReactiveCommand<Window, Unit> CopyQuillHashesCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowReadmeCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowWordTemplatesCommand { get; }
        #endregion

        private Database _database;
        public Database Database
        { 
            get => _database; 
            set => this.RaiseAndSetIfChanged(ref _database, value);
        }

        private string _status = "";
        public string Status
        {
            get => _status;
            private set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        #region Theme selection
        public bool UseDefaultTheme
        {
            get => Application.Current.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Default;
            set
            {
                AppSettings.Default.selectedTheme = "default";
                AppSettings.Default.Save();
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;
                this.RaisePropertyChanged(nameof(UseDefaultTheme));
                this.RaisePropertyChanged(nameof(UseLightTheme));
                this.RaisePropertyChanged(nameof(UseDarkTheme));
            }
        }

        public bool UseLightTheme
        {
            get => Application.Current.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Light;
            set
            {
                AppSettings.Default.selectedTheme = "light";
                AppSettings.Default.Save();
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                this.RaisePropertyChanged(nameof(UseDefaultTheme));
                this.RaisePropertyChanged(nameof(UseLightTheme));
                this.RaisePropertyChanged(nameof(UseDarkTheme));
            }
        }

        public bool UseDarkTheme
        {
            get => Application.Current.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
            set
            {
                AppSettings.Default.selectedTheme = "dark";
                AppSettings.Default.Save();
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                this.RaisePropertyChanged(nameof(UseDefaultTheme));
                this.RaisePropertyChanged(nameof(UseLightTheme));
                this.RaisePropertyChanged(nameof(UseDarkTheme));
            }
        }
        #endregion

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
            NewFileCommand = ReactiveCommand.CreateFromTask<Window>(newFileAsync);
            OpenFileCommand = ReactiveCommand.CreateFromTask<Window>(openFileAsync);
            QuitCommand = ReactiveCommand.CreateFromTask<Window>(QuitAsync);
            ShowAboutCommand = ReactiveCommand.CreateFromTask<Window>(showAboutAsync);
            ShowReadmeCommand = ReactiveCommand.Create(showReadmeWindow);
            ShowWordTemplatesCommand = ReactiveCommand.Create(showWordTemplates);
            ShowQuillHashesCommand = ReactiveCommand.CreateFromTask<Window>(showQuillHashesAsync);
            CopyQuillHashesCommand = ReactiveCommand.CreateFromTask<Window>(copyQuillHashesAsync);
            initialize(owner);
        }

        private async void initialize(Window dialogOwner)
        {
            // Delay to give main window a chance to show. ShowDialog below fails if
            // MainWindow isn't open yet.
            while (!dialogOwner.IsActive)
                await Task.Delay(100);

            // Check QuillJS hashes.
            bool quillVerified = await QuillJsEditor.VerifyHashes();
            // Was nice in theory, but somehow the hashing algorithm is sensitive to newline differences between Windows and Linux even though it operates in byte mode.
            // Disable for now.
            if (!quillVerified)
            {
                var msgbox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Editor Initialization Error",
                       "Editor assets failed SHA256 hash verification. Your installation" + Environment.NewLine +
                       "may have been corrupted by malware. You can continue to use the" + Environment.NewLine + 
                       "application, but you may face data corruption or malware issues." + Environment.NewLine +
                       "It is recommended you reinstall first.",
                    MsBox.Avalonia.Enums.ButtonEnum.OkAbort,
                    MsBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation.CenterOwner);
                var result = await msgbox.ShowWindowDialogAsync(dialogOwner);
                if (result == MsBox.Avalonia.Enums.ButtonResult.Abort)
                {
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(Program.DoubleClickedDatabaseFile) && File.Exists(Program.DoubleClickedDatabaseFile))
            {
                await openDatabaseAsync(Program.DoubleClickedDatabaseFile, dialogOwner);
            }
            else if (!string.IsNullOrWhiteSpace(AppSettings.Default.lastFile) && File.Exists(AppSettings.Default.lastFile))
            {
                await openDatabaseAsync(AppSettings.Default.lastFile, dialogOwner);
            }
        }

        private async Task newFileAsync(Window owner)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            SaveFileDialog dialog = new SaveFileDialog();
#pragma warning restore CS0618 // Type or member is obsolete
            dialog.Filters.Add(FileDialogFilters.DatabaseFilter);

            string path = await dialog.ShowAsync(owner);
            if (path != null)
            {
                Database = new Database(path);
                await Database.LoadAsync();
                AppSettings.Default.lastFile = Database.FileName;
                AppSettings.Default.Save();
            }
        }

        private async Task openFileAsync(Window owner)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            OpenFileDialog dialog = new OpenFileDialog();
#pragma warning restore CS0618 // Type or member is obsolete
            dialog.Filters.Add(FileDialogFilters.DatabaseFilter);
            dialog.AllowMultiple = false;

            string[] paths = await dialog.ShowAsync(owner);

            if (paths != null && paths.Length == 1 && File.Exists(paths[0]))
            {
                await openDatabaseAsync(paths[0], owner);
            }
        }

        private async Task openDatabaseAsync(string filename, Window dialogOwner)
        {
            Status = "Loading database " + filename + "...";
            Database = new Database(filename);

            if (Database.Metadata.DbVersion > Metadata.ExpectedVersion)
            {
                var msgBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Cannot Open File",
                    string.Format("Cannot open file '{0}'. Reason: file is version {1} but this application requires version {2}. The file will now be closed to prevent corruption.", Path.GetFileName(filename), Database.Metadata.DbVersion, Metadata.ExpectedVersion),
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation.CenterOwner);
                await msgBox.ShowWindowDialogAsync(dialogOwner);
                await Database.Close();
                Database = null;
                return;
            }

            if (Database.RequiresUpgrade)
            {
                DatabaseUpgradeView upgradeView = new DatabaseUpgradeView();
                upgradeView.DataContext = new DatabaseUpgradeViewModel(Database);
                // Delay to give main window a chance to show. ShowDialog below fails if
                // MainWindow isn't open yet.
                while (!dialogOwner.IsActive)
                    await Task.Delay(100);
                bool result = await upgradeView.ShowDialog<bool>(dialogOwner);
                if (!result)
                {
                    // Upgrade error.
                    (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
                    return;
                }
            }
            await Database.LoadAsync();
            AppSettings.Default.lastFile = filename;
            AppSettings.Default.Save();
            loadUniversesMenu();
            Database.Universes.CollectionChanged += (o, e) => loadUniversesMenu();
            Status = "";
        }

        private async Task showAboutAsync(Window owner)
        {
            await new AboutWindow().ShowDialog(owner);
        }

        public async Task QuitAsync(Window mainWindow)
        {
            if (OpenWindowsViewModel.AllWindows.Count() > 1)
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Open Windows",
                    "You have several open windows. Please save and close your work before closing the main window.",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(mainWindow);
                return;
            }

            Status = "Performing database cleanup...";
            AppSettings.Default.mainWindowWidth = mainWindow.Width;
            AppSettings.Default.mainWindowHeight = mainWindow.Height;
            AppSettings.Default.Save();

            if (Database != null)
                await Database.Close();

            (mainWindow as MainWindow).UnsubscribeClosingEvent();
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }

        private async Task showQuillHashesAsync(Window owner)
        {
            await new QuillHashWindow().ShowDialog(owner);
        }

        private async Task copyQuillHashesAsync(Window owner)
        {
            await QuillJsEditor.CopyHashesToClipboardAsync(owner);
            await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Hashes Copied",
                "QuillJS hash code copied to clipboard!",
                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Info,
                WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(owner);
        }

        private void showReadmeWindow()
        {
            ReadmeWindow.ShowReadmeWindow();
        }

        private void showWordTemplates()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = Path.Combine(Directory.GetCurrentDirectory(), "WordTemplates");
            Process.Start(psi);
        }

        private async Task selectUniverseAsync(Universe u)
        {
            Status = "Switching to universe " + u.Name + "...";
            foreach (var item in UniverseMenuItems)
            {
                item.IsChecked = item.Universe == u;
            }
            await Database.SelectUniverse(u);
            Status = "";
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
                    menuItem.Command = ReactiveCommand.CreateFromTask<Universe>(selectUniverseAsync);
                    UniverseMenuItems.Add(menuItem);
                }
            }
        }
    }
}
