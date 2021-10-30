using System;
using System.Windows;
using System.Windows.Threading;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for ConfirmDeleteDialog.xaml
    /// </summary>
    public partial class ConfirmDeleteDialog : Window
    {
        private string _message;
        private int secondsBeforeDelete = 3;
        private DispatcherTimer _timer;

        public ConfirmDeleteDialog(Window owner, string message)
        {
            InitializeComponent();
            Owner = owner;
            _message = message;
        }

        private void updateDeleteText()
        {
            if (secondsBeforeDelete <= 0)
            {
                _timer.Stop();
                deleteButton.IsEnabled = true;
                deleteButton.Content = "Delete";
            }
            else
            {
                deleteButton.Content = string.Format("Delete ({0})", secondsBeforeDelete);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DialogUtility.HideCloseButton(this);
            itemInfoTextBlock.Text = _message;
            updateDeleteText();
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            secondsBeforeDelete--;
            updateDeleteText();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
