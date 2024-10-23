using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels.Search
{
    public class NoteTitleSearchResult : SearchResult
    {
        private NoteDocumentViewModel _owner;
        public NoteDocumentViewModel Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                OnPropertyChanged("Owner");
                Name = _owner.Model.Name;
                Context = "";
            }
        }

        public NoteTitleSearchResult(SqliteDataReader sqlReader) : base(sqlReader)
        {
            ResultType = "Note Title";
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);
            EditCommand = ReactiveCommand.CreateFromTask<Window>(OpenEditor);
        }

        public async Task Rename(Window dialogOwner)
        {
            await _owner.Rename(dialogOwner);
            Name = Owner.Model.Name;
        }

        public async Task OpenEditor(Window dialogOwner)
        {
            NoteDocumentViewModel vm = Owner as NoteDocumentViewModel;
            NoteWindow.ShowEditorWindow(vm);
        }
    }
}
