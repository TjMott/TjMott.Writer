using Avalonia.Controls;
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
        public ReactiveCommand<Window, Unit> RenameCommand { get; }
        public ReactiveCommand<Window, Unit> DeleteCommand { get; }
        #endregion

        #region Properties
        public Category Model { get; private set; }
        public UniverseViewModel UniverseVm { get; set; }
        public SortBySortIndexBindingList<StoryViewModel> Stories { get; private set; }
        #endregion

        public async Task<long> GetWordCountAsync()
        {
            long wordCount = 0;
            foreach (var story in Stories)
            {
                wordCount += await story.GetWordCountAsync();
            }
            return wordCount;
        }
        public CategoryViewModel(Category model)
        {
            Model = model;
            Stories = new SortBySortIndexBindingList<StoryViewModel>();
            Model.PropertyChanged += Model_PropertyChanged;
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);
            DeleteCommand = ReactiveCommand.CreateFromTask<Window>(Delete);
        }

        public async Task Rename(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow(Model.Name);
            string result = await dialog.ShowDialog<string>(owner);
            if (result != null)
            {
                Model.Name = result;
                await Model.SaveAsync().ConfigureAwait(false);
            }
        }

        public async Task Delete(Window owner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Category: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.DeleteAsync().ConfigureAwait(false);
                await UniverseVm.DeleteSubItem(this);
            }
        }
    }
}
