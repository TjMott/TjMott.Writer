using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class EditNoteDocumentCategoriesWindow : Window
    {
        private NoteDocumentViewModel _document;
        private NotesTree _tree;

        private ObservableCollection<NoteCategoryViewModel> _availableCategories = new ObservableCollection<NoteCategoryViewModel>();
        private ObservableCollection<NoteCategoryViewModel> _selectedCategories = new ObservableCollection<NoteCategoryViewModel>();

        public EditNoteDocumentCategoriesWindow()
        {
            InitializeComponent();
        }

        public EditNoteDocumentCategoriesWindow(NoteDocumentViewModel document, NotesTree tree)
        {
            _tree = tree;
            _document = document;
            InitializeComponent();
        }

        private void EditNoteDocumentCategoriesWindow_Activated(object sender, System.EventArgs e)
        {
            this.Activated -= EditNoteDocumentCategoriesWindow_Activated;

            availableCategoriesListBox.ItemsSource = _availableCategories;
            selectedCategoriesListBox.ItemsSource = _selectedCategories;

            foreach (var category in _tree.Categories)
            {
                if (category.Children.Contains(_document))
                {
                    _selectedCategories.Add(category);
                }
                else
                {
                    _availableCategories.Add(category);
                }
            }
        }

        private void selectCategory_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (availableCategoriesListBox.SelectedItem != null && _selectedCategories.Count == 0)
            {
                NoteCategoryViewModel category  = (NoteCategoryViewModel)availableCategoriesListBox.SelectedItem;
                _availableCategories.Remove(category);
                _selectedCategories.Add(category);
            }
        }

        private void deselectCategory_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (selectedCategoriesListBox.SelectedItem != null)
            {
                NoteCategoryViewModel category = (NoteCategoryViewModel)selectedCategoriesListBox.SelectedItem;
                _selectedCategories.Remove(category);
                _availableCategories.Add(category);
            }
        }

        private async void ok_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await _tree.UpdateDocumentCategories(_document, _selectedCategories);
            Close();
        }

        private void cancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}
