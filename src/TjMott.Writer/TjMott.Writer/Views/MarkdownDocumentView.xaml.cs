using System;
using System.Windows.Controls;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Views
{
    /// <summary>
    /// Interaction logic for MarkdownDocumentView.xaml
    /// </summary>
    public partial class MarkdownDocumentView : UserControl
    {
        public MarkdownDocumentView()
        {
            InitializeComponent();
            DataContextChanged += MarkdownDocumentView_DataContextChanged;
        }

        private void MarkdownDocumentView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is MarkdownDocumentViewModel)
            {
                noDocTextBlock.Visibility = System.Windows.Visibility.Collapsed;
                documentPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                noDocTextBlock.Visibility = System.Windows.Visibility.Visible;
                documentPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void HtmlPanel_LinkClicked(object sender, TheArtOfDev.HtmlRenderer.WPF.RoutedEvenArgs<TheArtOfDev.HtmlRenderer.Core.Entities.HtmlLinkClickedEventArgs> args)
        {
            // This doesn't work...need to cancel the event so a browser doesn't open.
            args.Handled = true;
        }
    }
}
