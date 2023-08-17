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

            cefErrorTextBox.Text += Environment.NewLine + Environment.NewLine + "CEF Initialization Error Details: " + Environment.NewLine + CefNetAppImpl.InitErrorMessage;
        }
    }
}
