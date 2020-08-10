using System;
using System.Windows.Controls;
using System.Windows.Media;
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

        private void sceneColorMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            SceneViewModel scene = (SceneViewModel)menuItem.DataContext;
            scene.TextColorBrush = (SolidColorBrush)menuItem.Background;
        }
    }
}
