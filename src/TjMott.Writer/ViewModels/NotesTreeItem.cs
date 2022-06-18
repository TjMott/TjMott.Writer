using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;

namespace TjMott.Writer.ViewModels
{
    public abstract class NotesTreeItem : ViewModelBase
    {
        private NotesTreeItem _parent;
        public NotesTreeItem Parent
        {
            get { return _parent; }
            set { _parent = value; OnPropertyChanged("Parent"); }
        }

        public UniverseViewModel UniverseVm { get; protected set; }
        public ObservableCollection<NotesTreeItem> Children { get; private set; }

        public ReactiveCommand<Window, Unit> RenameCommand { get; }
        public ReactiveCommand<Window, Unit> DeleteCommand { get; }

        public NotesTreeItem()
        {
            Children = new ObservableCollection<NotesTreeItem>();
            RenameCommand = ReactiveCommand.Create<Window>(Rename);
            DeleteCommand = ReactiveCommand.Create<Window>(Delete);
        }

        public abstract void Rename(Window dialogOwner);
        public abstract void Delete(Window dialogOwner);
    }
}
