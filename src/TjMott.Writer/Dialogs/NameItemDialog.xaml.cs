using System;
using System.Windows;
using System.Windows.Input;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for NameItemDialog.xaml
    /// </summary>
    public partial class NameItemDialog : Window
    {
        public string UserInput { get; set; }

        public NameItemDialog(Window owner, string originalName)
        {
            InitializeComponent();
            Owner = owner;
            UserInput = originalName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DialogUtility.HideCloseButton(this);
            nameTextBox.Text = UserInput;
            nameTextBox.SelectAll();
            nameTextBox.Focus();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                UserInput = nameTextBox.Text;
                DialogResult = true;
                Close();
            }
            else
            {
                nameTextBox.Focus();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void nameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                okButton_Click(null, null);
            }
        }
    }
}
