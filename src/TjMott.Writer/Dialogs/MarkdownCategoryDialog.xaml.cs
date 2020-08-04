using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using TjMott.Writer.Model.SQLiteClasses;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for MarkdownCategoryDialog.xaml
    /// </summary>
    public partial class MarkdownCategoryDialog : Window
    {
        public BindingList<MarkdownCategory> Categories;
        private MarkdownCategory _category;

        public MarkdownCategoryDialog(Window owner, BindingList<MarkdownCategoryViewModel> categories, MarkdownCategory category)
        {
            InitializeComponent();
            Owner = owner;
            Categories = new BindingList<MarkdownCategory>();
            foreach (var c in categories)
            {
                if (c.Model.id != category.id)
                    Categories.Add(c.Model);
            }

            // Add "No category" option.
            MarkdownCategory noCat = new MarkdownCategory(category.Connection);
            noCat.id = -1;
            noCat.Name = "NONE";

            Categories.Insert(0, noCat);

            _category = category;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DialogUtility.HideCloseButton(this);
            categoryComboBox.ItemsSource = Categories;
            if (_category.ParentId == null)
            {
                categoryComboBox.SelectedItem = Categories[0];
            }
            else
            {
                categoryComboBox.SelectedItem = Categories.Single(i => i.id == _category.ParentId);
            }
            nameTextBox.Text = _category.Name;
            nameTextBox.SelectAll();
            nameTextBox.Focus();
        }


        private void nameTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                okay();
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            okay();
        }

        private void okay()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                nameTextBox.SelectAll();
                nameTextBox.Focus();
                return;
            }
            MarkdownCategory selectedCat = (MarkdownCategory)categoryComboBox.SelectedItem;
            if (selectedCat.id == -1)
                _category.ParentId = null;
            else
                _category.ParentId = selectedCat.id;
            _category.Name = nameTextBox.Text;
            this.DialogResult = true;
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
