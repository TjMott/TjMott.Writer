using Avalonia;
using Avalonia.Controls;

namespace TjMott.Writer.Views;

public partial class PasswordInputView : Window
{
    public PasswordInputView()
    {
        InitializeComponent();
        Initialized += PasswordInputView_Initialized;
    }

    private void PasswordInputView_Initialized(object sender, System.EventArgs e)
    {
        Initialized -= PasswordInputView_Initialized;
        passwordBox.Focus();
    }
}