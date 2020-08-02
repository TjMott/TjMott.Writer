using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TjMott.Writer.ViewModel
{
    public abstract class MarkdownTreeItem : ViewModelBase
    {
        private MarkdownTreeItem _parent;
        public MarkdownTreeItem Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                OnPropertyChanged("Parent");
            }
        }
        public UniverseViewModel UniverseVm { get; set; }
        public BindingList<MarkdownTreeItem> Children { get; private set; }

        public MarkdownTreeItem()
        {
            Children = new BindingList<MarkdownTreeItem>();
        }
    }
}
