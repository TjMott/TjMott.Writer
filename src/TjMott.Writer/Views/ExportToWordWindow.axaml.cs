using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using TjMott.Writer.Controls;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class ExportToWordWindow : Window
    {
        #region Instancing stuff
        private static Dictionary<IExportToWordDocument, ExportToWordWindow> _windows = new Dictionary<IExportToWordDocument, ExportToWordWindow>();
        public static void ShowExportWindow(IExportToWordDocument itemToExport)
        {
            try
            {
                if (!_windows.ContainsKey(itemToExport))
                {
                    ExportToWordWindow wnd = new ExportToWordWindow(itemToExport);
                    _windows[itemToExport] = wnd;
                }

                _windows[itemToExport].Show();
                _windows[itemToExport].Activate();
                _windows[itemToExport].Focus();
            }
            catch (ApplicationException)
            {

            }
        }
        #endregion

        public ExportToWordWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public ExportToWordWindow(IExportToWordDocument itemToExport)
        {
            DataContext = new ExportToWordViewModel(itemToExport);
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (!Avalonia.Controls.Design.IsDesignMode)
            {
                dynamic item = (DataContext as ExportToWordViewModel).ItemToExport;
                Title = item.Model.Name;
                OpenWindowsViewModel.Instance.ExportToWordWindows.Add(this);
            }
        }

        private void ExportToWordWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (OpenWindowsViewModel.Instance.ExportToWordWindows.Contains(this))
                OpenWindowsViewModel.Instance.ExportToWordWindows.Remove(this);
        }
    }
}
