using System;
using System.Data.SQLite;
using System.Windows.Input;

namespace TjMott.Writer.ViewModel.Search
{
    public class StorySearchResult : SearchResult
    {
        #region ICommands
        public override ICommand RenameCommand
        {
            get
            {
                if (_renameCommand == null)
                {
                    _renameCommand = new RelayCommand(param => Rename(), param => CanRename());
                }
                return _renameCommand;
            }
        }
        public override ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(param => Rename(), param => CanOpenEditor());
                }
                return _editCommand;
            }
        }
        #endregion

        #region Properties
        private StoryViewModel _owner;
        public StoryViewModel Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                OnPropertyChanged("Owner");
                Name = _owner.Model.Name;
                Context = "";
                if (_owner.Category != null)
                    Context = _owner.Category.Model.Name + " -> ";
                Context += _owner.Model.Name;
            }
        }
        #endregion

        public StorySearchResult(SQLiteDataReader sqlReader) : base(sqlReader)
        {
            ResultType = "Story Title";
        }

        public void Rename()
        {
            Owner.EditProperties();
            Name = Owner.Model.Name;
        }

        public bool CanRename()
        {
            return true;
        }

        public bool CanOpenEditor()
        {
            return false;
        }
    }
}
