using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public NotesTreeItem()
        {
            Children = new ObservableCollection<NotesTreeItem>();
        }
    }
}
