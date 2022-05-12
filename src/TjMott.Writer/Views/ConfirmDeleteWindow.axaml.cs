using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace TjMott.Writer.Views
{
    public partial class ConfirmDeleteWindow : Window
    {
        private int secondsBeforeDelete = 3;
        private DispatcherTimer _timer;

        public ConfirmDeleteWindow(string message)
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.FindControl<TextBlock>("itemInfoTextBlock").Text = message;
            this.FindControl<Button>("deleteButton").Content = string.Format("Delete ({0})", secondsBeforeDelete);
        }

        public ConfirmDeleteWindow()
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

        private void ConfirmDeleteWindow_Activated(object sender, System.EventArgs e)
        {
            this.Activated -= ConfirmDeleteWindow_Activated;
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();
        }

        private void _timer_Tick(object sender, System.EventArgs e)
        {
            secondsBeforeDelete--;
            updateDeleteText();
        }

        private void updateDeleteText()
        {
            if (secondsBeforeDelete <= 0)
            {
                _timer.Stop();
                this.FindControl<Button>("deleteButton").IsEnabled = true;
                this.FindControl<Button>("deleteButton").Content = "Delete";
            }
            else
            {
                this.FindControl<Button>("deleteButton").Content = string.Format("Delete ({0})", secondsBeforeDelete);
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
