using System;
using System.Data.SQLite;
using System.Windows.Input;
using TjMott.Writer.Dialogs;

namespace TjMott.Writer.ViewModel.Search
{
    public class ChapterSearchResult : SearchResult
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
        private ChapterViewModel _owner;
        public ChapterViewModel Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                OnPropertyChanged("Owner");
                Name = Owner.Model.Name;
                Context = "";
                if (_owner.StoryVm.Category != null)
                    Context = _owner.StoryVm.Category.Model.Name + " -> ";
                Context += _owner.StoryVm.Model.Name + " -> ";
                Context += _owner.Model.Name;
            }
        }
        #endregion

        public ChapterSearchResult(SQLiteDataReader sqlReader) : base(sqlReader)
        {
            ResultType = "Chapter Title";
        }

        public void Rename()
        {
            NameItemDialog dialog = new NameItemDialog(ViewModelBase.DialogOwner, Owner.Model.Name);
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult.Value)
            {
                Owner.Model.Name = dialog.UserInput;
                Owner.Model.Save();
                Name = Owner.Model.Name;
            }
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
