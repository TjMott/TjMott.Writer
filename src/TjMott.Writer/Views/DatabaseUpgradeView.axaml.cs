using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class DatabaseUpgradeView : Window
    {
        public DatabaseUpgradeView()
        {
            InitializeComponent();
            Closing += DatabaseUpgradeView_Closing;
        }

        private void DatabaseUpgradeView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = (DataContext as DatabaseUpgradeViewModel).IsBusy;
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown(1);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(true);
        }
    }
}
