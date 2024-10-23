using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace TjMott.Writer.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        /*private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<TextBlock>("versionTextBox").Text = GetType().Assembly.GetName().Version.ToString();

            this.FindControl<TextBlock>("dotNetInfoTextBlock").Text = string.Format("Runtime Info: .NET {0} on {1}.", System.Environment.Version.ToString(), System.Environment.OSVersion.VersionString);
        }*/

        private void okButton_Click(object sender, RoutedEventArgs args)
        {
            Close();
        }

        private void tjSiteButton_Click(object sender, RoutedEventArgs args)
        {
            Process.Start(new ProcessStartInfo("https://www.tjmott.com") {  UseShellExecute = true });
        }
        private void githubButton_Click(object sender, RoutedEventArgs args)
        {
            Process.Start(new ProcessStartInfo("https://github.com/TjMott/TjMott.Writer") { UseShellExecute = true });
        }

        private void iconsButton_Click(object sender, RoutedEventArgs args)
        {
            Process.Start(new ProcessStartInfo("https://icons8.com") { UseShellExecute = true });
        }
    }
}
