using Markdig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TjMott.Writer.Windows
{
    /// <summary>
    /// Interaction logic for ReadmeWindow.xaml
    /// </summary>
    public partial class ReadmeWindow : Window
    {
        private static ReadmeWindow _instance;
        public static void ShowInstance()
        {
            if (_instance == null)
            {
                _instance = new ReadmeWindow();
            }
            _instance.Show();
            _instance.Focus();
        }
        public ReadmeWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MarkdownPipelineBuilder builder = new MarkdownPipelineBuilder();
            builder.UsePipeTables();
            builder.UseSoftlineBreakAsHardlineBreak();
            builder.UseTaskLists();
            var pipeline = builder.Build();

            string readmeText = File.ReadAllText("README.md");
            string html = Markdown.ToHtml(readmeText, pipeline);
            htmlRenderer.Text = html;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _instance = null;
        }
    }
}
