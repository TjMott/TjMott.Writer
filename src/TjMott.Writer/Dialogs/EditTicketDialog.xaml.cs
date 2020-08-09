using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTicketDialog.xaml
    /// </summary>
    public partial class EditTicketDialog : Window
    {
        private TicketViewModel _viewModel;
        public EditTicketDialog(Window owner, TicketViewModel ticket)
        {
            InitializeComponent();
            Owner = owner;
            _viewModel = ticket;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DialogUtility.HideCloseButton(this);
            DataContext = _viewModel;
        }
    }
}
