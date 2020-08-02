using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class StoryViewModel : ViewModelBase, IUniverseSubItem, IExportToWordDocument, IGetWordCount
    {
        #region ISortable implementation - pass through to model
        public long SortIndex
        {
            get { return Model.SortIndex; }
            set { Model.SortIndex = value; }
        }
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SortIndex")
                OnPropertyChanged("SortIndex");
        }
        public void Save()
        {
            Model.Save();
        }
        #endregion

        #region ICommands
        private ICommand _editPropertiesCommand;
        public ICommand EditPropertiesCommand
        {
            get
            {
                if (_editPropertiesCommand == null)
                {
                    _editPropertiesCommand = new RelayCommand(param => EditProperties());
                }
                return _editPropertiesCommand;
            }
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => Delete());
                }
                return _deleteCommand;
            }
        }

        private ICommand _createChapterCommand;
        public ICommand CreateChapterCommand
        {
            get
            {
                if (_createChapterCommand == null)
                {
                    _createChapterCommand = new RelayCommand(param => CreateChapter());
                }
                return _createChapterCommand;
            }
        }
        #endregion

        public Story Model { get; private set; }
        public UniverseViewModel UniverseVm { get; set; }
        public SortBySortIndexBindingList<ChapterViewModel> Chapters { get; private set; }
        public StoryViewModel(Story model)
        {
            Model = model;
            Chapters = new SortBySortIndexBindingList<ChapterViewModel>();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public void EditProperties()
        {
            StoryPropertiesDialog dialog = new StoryPropertiesDialog(DialogOwner, Model, UniverseVm.Categories);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Save();
                UniverseVm.UpdateStoryInTree(this);
            }
        }

        public void Delete()
        {
            ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, string.Format("Story: {0}", Model.Name));
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                UniverseVm.DeleteSubItem(this);
            }
        }

        public void CreateChapter()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, "New Chapter");
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Chapter chapter = new Chapter(Model.Connection);
                chapter.StoryId = Model.id;
                chapter.Name = dialog.UserInput;
                if (Chapters.Count == 0)
                    chapter.SortIndex = 0;
                else
                    chapter.SortIndex = Chapters.Max(i => i.Model.SortIndex) + 1;
                chapter.Create();
                ChapterViewModel chapterVm = new ChapterViewModel(chapter);
                chapterVm.StoryVm = this;
                Chapters.Add(chapterVm);
            }
        }

        public void DeleteChapter(ChapterViewModel chapter)
        {
            Chapters.Remove(chapter);
            UpdateChapterSortIndices();
        }

        public void UpdateChapterSortIndices()
        {
            for (int i = 0; i < Chapters.Count; i++)
            {
                Chapters[i].Model.SortIndex = i;
                Chapters[i].Save();
            }
        }
    }
}
