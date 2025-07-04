using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TjMott.Writer.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            versionTextBox.Text = GetType().Assembly.GetName().Version.ToString();
            dotNetInfoTextBlock.Text = string.Format("Runtime Info: .NET {0} ({1}), on {2} ({3}).",
                System.Environment.Version.ToString(),
                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
                System.Environment.OSVersion.VersionString,
                System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
        }

        private void okButton_Click(object sender, RoutedEventArgs args)
        {
            Close();
        }
    }
}
