using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.ViewModels
{
    public class EditStoryPropertiesWindowViewModel : ViewModelBase
    {
        public Story Model { get; }

        public ReactiveCommand<Window, Unit> AcceptCommand { get; }
        public ReactiveCommand<Window, Unit> CancelCommand { get; }
        public ObservableCollection<CategoryViewModel> Categories { get; private set; }

        private CategoryViewModel _selectedCategory;
        public CategoryViewModel SelectedCategory { get => _selectedCategory; set => this.RaiseAndSetIfChanged(ref _selectedCategory, value); }

        public EditStoryPropertiesWindowViewModel(Story story, IEnumerable<CategoryViewModel> categories)
        {
            Model = story;
            AcceptCommand = ReactiveCommand.Create<Window>(accept);
            CancelCommand = ReactiveCommand.Create<Window>(cancel);

            Categories = new ObservableCollection<CategoryViewModel>();
            foreach (var c in categories)
                Categories.Add(c);

            // Add "No Category" option.
            Category noCat = new Category(Model.Connection);
            noCat.id = -1;
            noCat.Name = "NONE";
            CategoryViewModel noCatVm = new CategoryViewModel(noCat);
            Categories.Insert(0, noCatVm);

            if (Model.CategoryId == null)
                SelectedCategory = noCatVm;
            else
                SelectedCategory = Categories.Single(i => i.Model.id == Model.CategoryId);   
        }

        private void accept(Window wnd)
        {
            if (SelectedCategory.Model.id == -1)
                Model.CategoryId = null;
            else
                Model.CategoryId = SelectedCategory.Model.id;
            wnd.Close(true);
        }

        private void cancel(Window wnd)
        {
            wnd.Close(false);
        }
    }
}
