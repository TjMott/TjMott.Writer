using System;
using System.Windows.Controls;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Views
{
    /// <summary>
    /// Interaction logic for StoriesView.xaml
    /// </summary>
    public partial class StoriesView : UserControl
    {
        public StoriesView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            UniverseViewModel vm = DataContext as UniverseViewModel;
            if (vm != null)
            {
                vm.SelectedTreeViewItem = (sender as TreeView).SelectedItem;
            }
        }
    }
}
