using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace TjMott.Writer.Controls
{
    public partial class EditorCefErrorDisplay : UserControl
    {
        public EditorCefErrorDisplay()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (!CefNetAppImpl.InitSuccess)
            {
                this.FindControl<TextBox>("cefErrorTextBox").Text += Environment.NewLine + Environment.NewLine + "CEF Initialization Error Details: " + Environment.NewLine + CefNetAppImpl.InitErrorMessage;
            }
        }
    }
}
