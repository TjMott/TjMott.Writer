using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Text.Json;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.Views
{
    public partial class DocumentJsonWindow : Window
    {
        private Document _document;
        public DocumentJsonWindow()
        {
            InitializeComponent();
        }

        public DocumentJsonWindow(Document doc)
        {
            _document = doc;
            InitializeComponent();
            Activated += DocumentJsonWindow_Activated;
        }

        private void DocumentJsonWindow_Activated(object sender, System.EventArgs e)
        {
            Activated -= DocumentJsonWindow_Activated;
            reload();
        }

        private void ReloadMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            reload();
        }

        private void SaveMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {

        }

        private void CloseMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void reload()
        {
            await _document.LoadAsync();

            // Pretty-print the text.
            using var jsonDoc = JsonDocument.Parse(_document.PublicJson);
            var options = new JsonSerializerOptions() { WriteIndented = true };

            jsonTextBox.Text = JsonSerializer.Serialize(jsonDoc, options);
        }
    }
}
