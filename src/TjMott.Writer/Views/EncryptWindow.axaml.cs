using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TjMott.Writer.Views
{
    public partial class EncryptWindow : Window
    {
        public EncryptWindow()
        {
            InitializeComponent();
        }

        private void EncryptWindow_Activated(object sender, System.EventArgs e)
        {
            Activated -= EncryptWindow_Activated;
            this.FindControl<TextBox>("passwordBox").Focus();
        }

        private void okButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            confirm();
        }
        private void cancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private void passwordBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                confirm();
        }

        private async void confirm()
        {
            if (string.IsNullOrWhiteSpace(passwordBox.Text))
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Invalid Password", 
                    "Your password cannot be empty.", 
                    MsBox.Avalonia.Enums.ButtonEnum.Ok, 
                    MsBox.Avalonia.Enums.Icon.Warning, 
                    WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
                passwordBox.Focus();
                return;
            }
            else if (passwordBox.Text != confirmPasswordBox.Text)
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Password Mismatch",
                    "Your passwords do not match.",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Warning,
                    WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
                passwordBox.Text = "";
                confirmPasswordBox.Text = "";
                passwordBox.Focus();
                return;
            }
            else
            {
                Close(passwordBox.Text);
            }
        }
        
    }
}
