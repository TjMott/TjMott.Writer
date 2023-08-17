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
            passwordBox.Focus();
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
            TextBox p1 = passwordBox;
            TextBox p2 = confirmPasswordBox;

            if (string.IsNullOrWhiteSpace(p1.Text))
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Invalid Password", "Your password cannot be empty.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning, WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
                p1.Focus();
                return;
            }
            else if (p1.Text != p2.Text)
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Password Mismatch", "Your passwords do not match.", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning, WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(this);
                p1.Text = "";
                p2.Text = "";
                p1.Focus();
                return;
            }
            else
            {
                Close(p1.Text);
            }
        }
        
    }
}
