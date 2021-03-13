using System;
using System.Windows;

namespace TjMott.Writer.Windows
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        private bool _showConfirm;

        public string Password { get; private set; }

        public PasswordDialog(bool showConfirm = false)
        {
            InitializeComponent();
            _showConfirm = showConfirm;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(passwordBox.Password))
            {
                passwordBox.Focus();
                return;
            }
            if (_showConfirm && string.IsNullOrEmpty(confirmPasswordBox.Password))
            {
                confirmPasswordBox.Focus();
                return;
            }
            if (_showConfirm && confirmPasswordBox.Password != passwordBox.Password)
            {
                MessageBox.Show("Passwords do not match.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                passwordBox.Password = "";
                confirmPasswordBox.Password = "";
                passwordBox.Focus();
                return;
            }
            Password = passwordBox.Password;
            DialogResult = true;
            return;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_showConfirm)
            {
                Height = 150;
            }
            else
            {
                Height = 120;
                confirmGridRow.Height = new GridLength(0);
            }
            passwordBox.Focus();
        }
    }
}
