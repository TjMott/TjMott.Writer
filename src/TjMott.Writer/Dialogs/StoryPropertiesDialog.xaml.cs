using System;
using System.Collections.Generic;
using System.Windows;
using TjMott.Writer.Model.SQLiteClasses;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for StoryPropertiesDialog.xaml
    /// </summary>
    public partial class StoryPropertiesDialog : Window
    {
        private StoryPropertiesViewModel _viewModel;

        public StoryPropertiesDialog(Window owner, Story story, IEnumerable<CategoryViewModel> availableCategories)
        {
            InitializeComponent();
            Owner = owner;
            _viewModel = new StoryPropertiesViewModel(story, this, availableCategories);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DialogUtility.HideCloseButton(this);
            DataContext = _viewModel;
        }
    }
}
