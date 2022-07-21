using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TjMott.Writer.Views
{
    public partial class DecryptWindow : Window
    {
        public DecryptWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DecryptWindow_Activated(object sender, System.EventArgs e)
        {
            Activated -= DecryptWindow_Activated;
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
            TextBox p1 = this.FindControl<TextBox>("passwordBox");

            if (string.IsNullOrWhiteSpace(p1.Text))
            {
                await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Invalid Password", "Your password cannot be empty.", MessageBox.Avalonia.Enums.ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Warning, WindowStartupLocation.CenterOwner).ShowDialog(this);
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
