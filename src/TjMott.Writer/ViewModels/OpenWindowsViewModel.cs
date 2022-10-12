using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace TjMott.Writer.ViewModels
{
    public class OpenWindowsViewModel : ViewModelBase
    {
        public class OpenWindowsViewModelItem
        {
            public Window Window { get; private set; }
            public string Name { get; private set; }

            public OpenWindowsViewModelItem(Window wnd, string name)
            {
                Window = wnd;
                Name = name;
            }

            public void Activate()
            {
                Window.WindowState = WindowState.Normal;
                Window.Show();
                Window.Activate();
                Window.Focus();
            }
        }
        public static OpenWindowsViewModel Instance { get; private set; }

        private Window _mainWindow;
        public Window MainWindow { 
            get => _mainWindow;
            set
            {
                this.RaiseAndSetIfChanged(ref _mainWindow, value);
                this.RaisePropertyChanged(nameof(AllWindows));
            }
        }

        private ObservableCollection<Window> _editorWindows;
        public ObservableCollection<Window> EditorWindows { get => _editorWindows; private set => this.RaiseAndSetIfChanged(ref _editorWindows, value); }
        private ObservableCollection<Window> _copyrightWindows;
        public ObservableCollection<Window> CopyrightWindows { get => _copyrightWindows; private set => this.RaiseAndSetIfChanged(ref _copyrightWindows, value); }


        private ObservableCollection<Window> _notesWindows;
        public ObservableCollection<Window> NotesWindows { get => _notesWindows; private set => this.RaiseAndSetIfChanged(ref _notesWindows, value); }

        private ObservableCollection<Window> _pacingWindows;
        public ObservableCollection<Window> PacingWindows { get => _pacingWindows; private set => this.RaiseAndSetIfChanged(ref _pacingWindows, value); }

        private ObservableCollection<Window> _exportToWordWindows;
        public ObservableCollection<Window> ExportToWordWindows { get => _exportToWordWindows; private set => this.RaiseAndSetIfChanged(ref _exportToWordWindows, value); }

        private Window _readmeWindow;
        public Window ReadmeWindow 
        { 
            get => _readmeWindow;
            set
            {
                this.RaiseAndSetIfChanged(ref _readmeWindow, value);
                this.RaisePropertyChanged(nameof(AllWindows));
            }
        }

        public OpenWindowsViewModel(Window mainWindow)
        {
            Instance = this;
            MainWindow = mainWindow;
            EditorWindows = new ObservableCollection<Window>();
            CopyrightWindows = new ObservableCollection<Window>();
            NotesWindows = new ObservableCollection<Window>();
            ExportToWordWindows = new ObservableCollection<Window>();
            PacingWindows = new ObservableCollection<Window>();

            EditorWindows.CollectionChanged += (o, e) => this.RaisePropertyChanged(nameof(AllWindows));
            NotesWindows.CollectionChanged += (o, e) => this.RaisePropertyChanged(nameof(AllWindows));
            PacingWindows.CollectionChanged += (o, e) => this.RaisePropertyChanged(nameof(AllWindows));
            CopyrightWindows.CollectionChanged += (o, e) => this.RaisePropertyChanged(nameof(AllWindows));
            ExportToWordWindows.CollectionChanged += (o, e) => this.RaisePropertyChanged(nameof(AllWindows));
        }

        public IEnumerable<OpenWindowsViewModelItem> AllWindows
        {
            get
            {
                List<OpenWindowsViewModelItem> windows = new List<OpenWindowsViewModelItem>();

                windows.Add(new OpenWindowsViewModelItem(MainWindow, "Main Window - TJ Mott's Writer"));

                foreach (var w in EditorWindows)
                {
                    windows.Add(new OpenWindowsViewModelItem(w, "Document - " + w.Title));
                }
                foreach (var w in CopyrightWindows)
                {
                    windows.Add(new OpenWindowsViewModelItem(w, "Copyright Page - " + w.Title));
                }

                foreach (var w in NotesWindows)
                {
                    windows.Add(new OpenWindowsViewModelItem(w, "Note - " + w.Title));
                }

                foreach (var w in PacingWindows)
                {
                    windows.Add(new OpenWindowsViewModelItem(w, w.Title));
                }

                foreach (var w in ExportToWordWindows)
                {
                    windows.Add(new OpenWindowsViewModelItem(w, "Export - " + w.Title));
                }

                if (ReadmeWindow != null)
                    windows.Add(new OpenWindowsViewModelItem(ReadmeWindow, "Readme"));

                return windows;
            }
        }
    }
}
