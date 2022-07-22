using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using System.Threading.Tasks;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels.Search
{
    public class SceneTitleSearchResult : SearchResult
    {
        private ViewModelBase _owner;
        public ViewModelBase Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                OnPropertyChanged("Owner");
                if (_owner is SceneViewModel)
                {
                    SceneViewModel scene = (SceneViewModel)_owner;
                    Name = scene.Model.Name;
                    ResultType = "Scene Title";
                    Context = "";
                    if (scene.ChapterVm.StoryVm.Category != null)
                        Context = scene.ChapterVm.StoryVm.Category.Model.Name + " -> ";
                    Context += scene.ChapterVm.StoryVm.Model.Name + " -> ";
                    Context += scene.ChapterVm.Model.Name + " -> ";
                    Context += scene.Model.Name;
                }
            }
        }
        public SceneTitleSearchResult(SqliteDataReader sqlReader) : base(sqlReader)
        {
            ResultType = "";
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);
            EditCommand = ReactiveCommand.CreateFromTask<Window>(OpenEditor);
        }

        public async Task Rename(Window dialogOwner)
        {
            if (Owner is SceneViewModel)
            {
                SceneViewModel scene = (SceneViewModel)Owner;
                await scene.Rename(dialogOwner);
                Name = scene.Model.Name;
            }
        }

        public async Task OpenEditor(Window dialogOwner)
        {
            if (Owner is SceneViewModel)
            {
                SceneViewModel vm = Owner as SceneViewModel;
                SceneEditorWindow.ShowEditorWindow(vm);
            }
        }
    }
}
