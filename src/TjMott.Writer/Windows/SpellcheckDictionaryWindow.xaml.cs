using System;
using System.Windows;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Windows
{
    /// <summary>
    /// Interaction logic for SpellcheckDictionaryWindow.xaml
    /// </summary>
    public partial class SpellcheckDictionaryWindow : Window
    {
        #region Singleton stuff
        private static SpellcheckDictionaryWindow _instance;
        public static void ShowSpellcheckDictionaryWindow(SpellcheckDictionary vm)
        {
            if (_instance == null)
            {
                _instance = new SpellcheckDictionaryWindow();
            }
            _instance.DataContext = vm;
            _instance.Show();
            _instance.Focus();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _instance = null;
        }

        #endregion
        public SpellcheckDictionaryWindow()
        {
            InitializeComponent();
        }

        
    }
}
