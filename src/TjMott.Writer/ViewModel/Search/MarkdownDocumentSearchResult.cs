using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.Windows.Input;

namespace TjMott.Writer.ViewModel.Search
{
    public class MarkdownDocumentSearchResult : SearchResult
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
                    _editCommand = new RelayCommand(param => OpenEditor(), param => CanOpenEditor());
                }
                return _editCommand;
            }
        }
        #endregion

        #region Properties

        private ViewModelBase _owner;
        public ViewModelBase Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                OnPropertyChanged("Owner");

                if (Owner is MarkdownDocumentViewModel)
                {
                    MarkdownDocumentViewModel vm = (MarkdownDocumentViewModel)Owner;
                    ResultType = "Note Text";
                    Name = vm.Model.Name;
                    Context = "Regular Note";
                }
                else if (Owner is TicketViewModel)
                {
                    TicketViewModel vm = (TicketViewModel)Owner;
                    ResultType = "Ticket Description";
                    Name = vm.Model.Name;
                    Context = string.Format("Ticket {0}", vm.Model.id);
                }
                else if (Owner is CategoryViewModel)
                {
                    CategoryViewModel vm = (CategoryViewModel)Owner;
                    ResultType = "Category Note";
                    Name = vm.Model.Name;
                    Context = string.Format("Category {0}", vm.Model.Name);
                }
                else if (Owner is StoryViewModel)
                {
                    StoryViewModel vm = (StoryViewModel)Owner;
                    ResultType = "Story Note";
                    Name = vm.Model.Name;
                    Context = string.Format(vm.Model.Name);
                }
                else if (Owner is ChapterViewModel)
                {
                    ChapterViewModel vm = (ChapterViewModel)Owner;
                    ResultType = "Chapter Note";
                    Name = vm.Model.Name;
                    Context = string.Format("{0} -> {1}", vm.StoryVm.Model.Name, vm.Model.Name);
                }
                else if (Owner is SceneViewModel)
                {
                    SceneViewModel vm = (SceneViewModel)Owner;
                    ResultType = "Scene Note";
                    Name = vm.Model.Name;
                    Context = string.Format("{0} -> {1} -> {2}", vm.ChapterVm.StoryVm.Model.Name, vm.ChapterVm.Model.Name, vm.Model.Name);
                }
            }
        }
        #endregion

        public MarkdownDocumentSearchResult(SQLiteDataReader sqlReader) : base(sqlReader)
        {
            
        }

        public void Rename()
        {

        }

        public bool CanRename()
        {
            return false;
        }

        public void OpenEditor()
        {
            if (Owner is MarkdownDocumentViewModel)
            {
                (Owner as MarkdownDocumentViewModel).OpenInWindow();
            }
            else if (Owner is TicketViewModel)
            {
                (Owner as TicketViewModel).Edit();
            }
        }

        public bool CanOpenEditor()
        {
            return true;
        }
    }
}
