using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using TjMott.Writer.Model.SQLiteClasses;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectDocTemplateDialog.xaml
    /// </summary>
    public partial class SelectDocTemplateDialog : Window
    {
        private BindingList<FileBlobViewModel> _templates;

        private FileBlobViewModel _selectedTemplate;
        public FileBlobViewModel SelectedTemplate
        {
            get
            {
                if (_selectedTemplate.Model.id == -1)
                    return null;
                else
                    return _selectedTemplate;
            }
        }

        public SelectDocTemplateDialog(Window owner, IEnumerable<FileBlobViewModel> templates, FileBlobViewModel defaultTemplate)
        {
            InitializeComponent();
            Owner = owner;

            _templates = new BindingList<FileBlobViewModel>();
            foreach (var item in templates)
            {
                _templates.Add(item);
            }

            // Add a no-template option.
            FileBlob noTemplate = new FileBlob(templates.First().Model.Connection);
            noTemplate.id = -1;
            noTemplate.Name = "NONE";

            FileBlobViewModel noVm = new FileBlobViewModel(noTemplate, null);
            _templates.Insert(0, noVm);
            if (defaultTemplate == null)
                _selectedTemplate = noVm;
            else
                _selectedTemplate = defaultTemplate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DialogUtility.HideCloseButton(this);

            templateListBox.ItemsSource = _templates;
            templateListBox.SelectedItem = _selectedTemplate;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedTemplate = (FileBlobViewModel)templateListBox.SelectedItem;
            this.DialogResult = true;
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
