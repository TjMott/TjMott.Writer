using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TjMott.Writer.ViewModel
{
    public class EditMarkdownDocumentCategoriesViewModel : ViewModelBase
    {
        #region Properties
        public BindingList<MarkdownCategoryViewModel> AvailableCategories { get; private set; }
        public BindingList<MarkdownCategoryViewModel> SelectedCategories { get; private set; }

        public MarkdownDocumentViewModel Document { get; private set; }
        public MarkdownTree MarkdownTree { get; private set; }
        public Window Owner { get; private set; }

        private MarkdownCategoryViewModel _selectedAvailableCategory;
        public MarkdownCategoryViewModel SelectedAvailableCategory
        {
            get { return _selectedAvailableCategory; }
            set
            {
                _selectedAvailableCategory = value;
                OnPropertyChanged("SelectedAvailableCategory");
            }
        }

        private MarkdownCategoryViewModel _selectedUsedCategory;
        public MarkdownCategoryViewModel SelectedUsedCategory
        {
            get { return _selectedUsedCategory; }
            set
            {
                _selectedUsedCategory = value;
                OnPropertyChanged("SelectedUsedCategory");
            }
        }
        #endregion
        private ICommand _selectCategoryCommand;
        public ICommand SelectCategoryCommand
        {
            get
            {
                if (_selectCategoryCommand == null)
                {
                    _selectCategoryCommand = new RelayCommand(param => SelectCategory(), param => CanSelectCategory());
                }
                return _selectCategoryCommand;
            }
        }
        private ICommand _deselectCategoryCommand;
        public ICommand DeselectCategoryCommand
        {
            get
            {
                if (_deselectCategoryCommand == null)
                {
                    _deselectCategoryCommand = new RelayCommand(param => DeselectCategory(), param => CanDeselectCategory());
                }
                return _deselectCategoryCommand;
            }
        }
        #region ICommands

        #endregion

        public EditMarkdownDocumentCategoriesViewModel(Window owner, MarkdownDocumentViewModel document, MarkdownTree tree)
        {
            Owner = owner;
            MarkdownTree = tree;
            Document = document;
            AvailableCategories = new BindingList<MarkdownCategoryViewModel>();
            SelectedCategories = new BindingList<MarkdownCategoryViewModel>();

            foreach (var category in MarkdownTree.Categories)
            {
                if (category.Children.Contains(Document))
                {
                    SelectedCategories.Add(category);
                }
                else
                {
                    AvailableCategories.Add(category);
                }
            }
        }

        public void SelectCategory()
        {
            SelectedCategories.Add(SelectedAvailableCategory);
            AvailableCategories.Remove(SelectedAvailableCategory);
            SelectedAvailableCategory = null;
        }

        public bool CanSelectCategory()
        {
            return SelectedAvailableCategory != null;
        }

        public void DeselectCategory()
        {
            AvailableCategories.Add(SelectedUsedCategory);
            SelectedCategories.Remove(SelectedUsedCategory);
            SelectedUsedCategory = null;
        }

        public bool CanDeselectCategory()
        {
            return SelectedUsedCategory != null;
        }
    }
}
