using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace TjMott.Writer.Views
{
    public partial class NameItemWindow : Window
    {

        private string _userInput;
        public NameItemWindow()
        {
            _userInput = "";
            InitializeComponent();
        }

        public NameItemWindow(string userInput)
        {
            _userInput = userInput;
            InitializeComponent();
            nameTextBox.Text = _userInput;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                _userInput = nameTextBox.Text;
                Close(_userInput);
            }
            else
            {
                nameTextBox.Focus();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
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
