using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using System.Threading.Tasks;

namespace TjMott.Writer.ViewModels.Search
{
    public class StoryTitleSearchResult : SearchResult
    {
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

        public StoryTitleSearchResult(SqliteDataReader sqlReader) : base(sqlReader)
        {
            ResultType = "Story Title";
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);

            // Dummy command that is never enabled, since this isn't used.
            EditCommand = ReactiveCommand.CreateFromTask<Window>(Rename, this.WhenAny(i => i, i => false));
        }

        public async Task Rename(Window dialogOwner)
        {
            await Owner.EditProperties(dialogOwner);
            Name = Owner.Model.Name;
        }
    }
}
