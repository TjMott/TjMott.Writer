using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.IO;
using System.Reactive;
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
        }

        public async void NewFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filters.Add(FileDialogFilters.DatabaseFilter);

            string path = await dialog.ShowAsync(MainWindow);
            if (path != null)
            {
                Database = new Database(path);
                Database.Load();
                
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
                Database = new Database(paths[0]);

                if (Database.RequiresUpgrade)
                {

                }
                else
                {
                    Database.Load();
                }
            }
        }

        public void ShowAbout()
        {
            AboutWindow wnd = new AboutWindow();
            wnd.ShowDialog(MainWindow);
        }

        public void Quit()
        {
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }
    }
}
