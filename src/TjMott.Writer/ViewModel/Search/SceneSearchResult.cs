using System;
using System.Data.SQLite;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Windows;

namespace TjMott.Writer.ViewModel.Search
{
    public class SceneSearchResult : SearchResult
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
        #endregion

        public SceneSearchResult(SQLiteDataReader sqlReader) : base(sqlReader)
        {
            ResultType = "";
        }

        public void Rename()
        {
            if (Owner is SceneViewModel)
            {
                SceneViewModel scene = (SceneViewModel)Owner;
                NameItemDialog dialog = new NameItemDialog(ViewModelBase.DialogOwner, scene.Model.Name);
                bool? dialogResult = dialog.ShowDialog();
                if (dialogResult.Value)
                {
                    scene.Model.Name = dialog.UserInput;
                    scene.Model.Save();
                    Name = scene.Model.Name;
                }
            }
        }

        public bool CanRename()
        {
            return true;
        }

        public void OpenEditor()
        {
            if (Owner is SceneViewModel)
            {
                SceneViewModel vm = Owner as SceneViewModel;
                FlowDocumentEditorWindow.ShowEditorWindow(vm.Model.FlowDocumentId, vm.Model.Connection, vm.ChapterVm.StoryVm.UniverseVm.SpellcheckDictionary, string.Format("Scene: {0}", vm.Model.Name), SnippetResult);
            }
        }

        public bool CanOpenEditor()
        {
            return Owner is SceneViewModel;
        }
    }
}
