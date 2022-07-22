using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
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
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);
            DeleteCommand = ReactiveCommand.CreateFromTask<Window>(Delete);
        }

        public abstract Task Rename(Window dialogOwner);
        public abstract Task Delete(Window dialogOwner);

        public abstract Task SetCategories(Window dialogOwner);
    }
}
