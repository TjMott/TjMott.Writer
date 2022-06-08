using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

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
        public async Task SaveAsync()
        {
            await Model.SaveAsync().ConfigureAwait(false);
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

        public async void Rename()
        {
            NameItemWindow dialog = new NameItemWindow(Model.Name);
            string result = await dialog.ShowDialog<string>(MainWindow);
            if (result != null)
            {
                Model.Name = result;
                await Model.SaveAsync().ConfigureAwait(false);
            }
        }

        public async void Delete()
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Category: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(MainWindow);
            if (result)
            {
                await Model.DeleteAsync().ConfigureAwait(false);
                UniverseVm.DeleteSubItem(this);
            }
        }

        public async void UpdateStorySortIndices()
        {
            for (int i = 0; i < Stories.Count; i++)
            {
                Stories[i].Model.SortIndex = i;
                await Stories[i].SaveAsync().ConfigureAwait(false);
            }
        }
    }
}
