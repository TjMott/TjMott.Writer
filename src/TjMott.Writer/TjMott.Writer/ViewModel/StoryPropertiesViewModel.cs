using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class StoryPropertiesViewModel : ViewModelBase
    {
        #region ICommands
        private ICommand _acceptCommand;
        public ICommand AcceptCommand
        {
            get
            {
                if (_acceptCommand == null)
                {
                    _acceptCommand = new RelayCommand(param => Accept());
                }
                return _acceptCommand;
            }
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(param => Cancel());
                }
                return _cancelCommand;
            }
        }
        #endregion

        private CategoryViewModel _selectedCategory;
        public CategoryViewModel SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                OnPropertyChanged("SelectedCategory");
            }
        }

        public List<CategoryViewModel> Categories { get; private set; }
        public Story Model { get; private set; }
        private Window _window;

        public StoryPropertiesViewModel(Story model, Window window, IEnumerable<CategoryViewModel> availableCategories)
        {
            Model = model;
            _window = window;

            // Add "No category" option.
            Categories = availableCategories.ToList();
            Category noCat = new Category(model.Connection);
            noCat.id = -1;
            noCat.Name = "NONE";
            CategoryViewModel noCatVm = new CategoryViewModel(noCat);

            Categories.Insert(0, noCatVm);

            if (model.CategoryId == null)
                SelectedCategory = noCatVm;
            else
            {
                foreach (var cat in Categories)
                {
                    if (cat.Model.id == model.CategoryId)
                        SelectedCategory = cat;
                }
            }
        }

        public void Accept()
        {
            if (SelectedCategory.Model.id == -1)
                Model.CategoryId = null;
            else
                Model.CategoryId = SelectedCategory.Model.id;
            _window.DialogResult = true;
            _window.Close();
        }

        public void Cancel()
        {
            _window.DialogResult = false;
            _window.Close();
        }
    }
}
