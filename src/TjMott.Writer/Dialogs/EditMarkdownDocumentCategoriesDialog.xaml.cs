using System;
using System.Windows;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for EditMarkdownDocumentCategoriesDialog.xaml
    /// </summary>
    public partial class EditMarkdownDocumentCategoriesDialog : Window
    {
        private EditMarkdownDocumentCategoriesViewModel _viewModel;

        public EditMarkdownDocumentCategoriesDialog(Window owner, MarkdownDocumentViewModel doc, MarkdownTree tree)
        {
            InitializeComponent();
            Owner = owner;

            _viewModel = new EditMarkdownDocumentCategoriesViewModel(this, doc, tree);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;
        }
    }
}
