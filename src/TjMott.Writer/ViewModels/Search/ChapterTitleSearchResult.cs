using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using System.Threading.Tasks;

namespace TjMott.Writer.ViewModels.Search
{
    public class ChapterTitleSearchResult : SearchResult
    {
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

        public ChapterTitleSearchResult(SqliteDataReader sqlReader) : base(sqlReader)
        {
            ResultType = "Chapter Title";
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);

            // Dummy command that is never enabled, since this isn't used.
            EditCommand = ReactiveCommand.CreateFromTask<Window>(Rename, this.WhenAny(i => i, i => false));
        }

        public async Task Rename(Window dialogOwner)
        {
            await _owner.RenameAsync(dialogOwner);
            Name = _owner.Model.Name;
        }
    }
}
