using System;
using System.Windows;
using TjMott.Writer.Properties;
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
            Width = AppSettings.Default.ticketDialogWidth;
            Height = AppSettings.Default.ticketDialogHeight;
            DialogUtility.HideCloseButton(this);
            DataContext = _viewModel;
            Title = "Editing Ticket " + _viewModel.Model.id;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.Default.ticketDialogHeight = ActualHeight;
            AppSettings.Default.ticketDialogWidth = ActualWidth;
            AppSettings.Default.Save();
        }
    }
}
