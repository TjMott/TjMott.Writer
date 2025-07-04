using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using System.Threading.Tasks;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels.Search
{
    public class DocumentTextSearchResult : SearchResult
    {
        private ViewModelBase _owner;
        public ViewModelBase Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                OnPropertyChanged(nameof(Owner));
                if (_owner is SceneViewModel)
                {
                    SceneViewModel scene = (SceneViewModel)_owner;
                    Name = scene.Model.Name;
                    ResultType = "Scene Text";
                    Context = "";
                    if (scene.ChapterVm.StoryVm.Category != null)
                        Context = scene.ChapterVm.StoryVm.Category.Model.Name + " -> ";
                    Context += scene.ChapterVm.StoryVm.Model.Name + " -> ";
                    Context += scene.ChapterVm.Model.Name + " -> ";
                    Context += scene.Model.Name;
                }
                else if (_owner is StoryViewModel)
                {
                    StoryViewModel story = (StoryViewModel)_owner;
                    Name = story.Model.Name;
                    ResultType = "Copyright Page";
                    Context = story.Model.Name = " copyright page";
                }
            }
        }

        public DocumentTextSearchResult(SqliteDataReader sqlReader) : base(sqlReader)
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
                NameItemWindow dialog = new NameItemWindow(scene.Model.Name);
                string newName = await dialog.ShowDialog<string>(dialogOwner);
                if (!string.IsNullOrEmpty(newName))
                {
                    scene.Model.Name = newName;
                    await scene.Model.SaveAsync();
                    Name = scene.Model.Name;
                }
            }
        }

        public async Task OpenEditor(Window dialogOwner)
        {
            await Task.Yield();
            if (Owner is SceneViewModel)
            {
                SceneViewModel vm = Owner as SceneViewModel;
                // TODO: Pass in SnippetResult to search for in Quill...?
                SceneEditorWindow.ShowEditorWindow(vm);
            }
            else if (Owner is StoryViewModel)
            {
                StoryViewModel story = Owner as StoryViewModel;
                EditCopyrightPageWindow.ShowEditorWindow(story);
            }
        }
    }
}
