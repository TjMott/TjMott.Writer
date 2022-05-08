using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.ViewModels
{
    public class CategoryViewModel : ViewModelBase, IUniverseSubItem, IGetWordCount
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
        public ReactiveCommand<Unit, Unit> RenameCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        #endregion

        #region Properties
        public Category Model { get; private set; }
        public UniverseViewModel UniverseVm { get; set; }
        public SortBySortIndexBindingList<StoryViewModel> Stories { get; private set; }
        #endregion

        public long GetWordCount()
        {
            return Stories.Sum(i => i.GetWordCount());
        }
        public CategoryViewModel(Category model)
        {
            Model = model;
            Stories = new SortBySortIndexBindingList<StoryViewModel>();
            Model.PropertyChanged += Model_PropertyChanged;
            RenameCommand = ReactiveCommand.Create(Rename);
            DeleteCommand = ReactiveCommand.Create(Delete);
        }

        public void Rename()
        {
           /* NameItemDialog dialog = new NameItemDialog(DialogOwner, Model.Name);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Name = dialog.UserInput;
                Model.Save();
            }*/
        }

        public void Delete()
        {
            /*ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, string.Format("Category: {0}", Model.Name));
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                UniverseVm.DeleteSubItem(this);
            }*/
        }

        public void UpdateStorySortIndices()
        {
            for (int i = 0; i < Stories.Count; i++)
            {
                Stories[i].Model.SortIndex = i;
                Stories[i].Save();
            }
        }
    }
}
