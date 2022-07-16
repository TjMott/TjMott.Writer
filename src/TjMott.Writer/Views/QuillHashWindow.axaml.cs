using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TjMott.Writer.Controls;

namespace TjMott.Writer.Views
{
    public partial class QuillHashWindow : Window
    {
        public QuillHashWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Activated += QuillHashWindow_Activated;
        }

        private void QuillHashWindow_Activated(object sender, System.EventArgs e)
        {
            Activated -= QuillHashWindow_Activated;

            DataGrid dg = this.FindControl<DataGrid>("hashesDataGrid");
            dg.Items = QuillJsEditor.AssetHashes;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
