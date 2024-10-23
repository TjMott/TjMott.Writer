using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace TjMott.Writer.Views
{
    public partial class NameItemWindow : Window
    {

        private string _userInput = "";
        public NameItemWindow()
        {
            InitializeComponent();
        }

        public NameItemWindow(string userInput)
        {
            _userInput = userInput;
            InitializeComponent();
        }

        /*private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<TextBox>("nameTextBox").Text = _userInput;
        }*/

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.FindControl<TextBox>("nameTextBox").Text))
            {
                _userInput = this.FindControl<TextBox>("nameTextBox").Text;
                Close(_userInput);
            }
            else
            {
                this.FindControl<TextBox>("nameTextBox").Focus();
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
