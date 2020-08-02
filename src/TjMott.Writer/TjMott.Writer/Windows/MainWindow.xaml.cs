﻿using System;
using System.Windows;
using TjMott.Writer.Properties;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private variables
        private MainWindowViewModel _viewModel;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Width = AppSettings.Default.mainWindowWidth;
            Height = AppSettings.Default.mainWindowHeight;

            MainWindowViewModel.DialogOwner = this;

            _viewModel = new MainWindowViewModel(this);
            DataContext = _viewModel;

            if (!string.IsNullOrWhiteSpace(App.StartupFileName))
            {
                try
                {
                    if (System.IO.File.Exists(App.StartupFileName))
                    {
                        _viewModel.OpenDatabase(App.StartupFileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Opening File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettings.Default.mainWindowWidth = Width;
            AppSettings.Default.mainWindowHeight = Height;
            AppSettings.Default.Save();

            _viewModel.Exit();

            MarkdownDocumentWindow.CloseAllWindows();
        }
        #endregion
    }
}
