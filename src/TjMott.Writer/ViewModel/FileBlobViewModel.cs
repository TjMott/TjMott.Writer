using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class FileBlobViewModel : ViewModelBase, ISortable
    {
        #region ISortable implementation - pass through to model
        public long SortIndex
        {
            get { return Model.SortIndex; }
            set { Model.SortIndex = value; }
        }
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SortIndex")
                OnPropertyChanged("SortIndex");
        }
        public void Save()
        {
            Model.Save();
        }
        #endregion

        #region ICommands
        private ICommand _extractCommand;
        public ICommand ExtractCommand
        {
            get
            {
                if (_extractCommand == null)
                {
                    _extractCommand = new RelayCommand(param => Extract());
                }
                return _extractCommand;
            }
        }
        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => Delete());
                }
                return _deleteCommand;
            }
        }
        private ICommand _renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                if (_renameCommand == null)
                {
                    _renameCommand = new RelayCommand(param => Rename());
                }
                return _renameCommand;
            }
        }
        private ICommand _replaceCommand;
        public ICommand ReplaceCommand
        {
            get
            {
                if (_replaceCommand == null)
                {
                    _replaceCommand = new RelayCommand(param => Replace());
                }
                return _replaceCommand;
            }
        }
        #endregion

        public FileBlob Model { get; private set; }
        public FileBrowserViewModel FileBrowserVm { get; private set; }

        public FileBlobViewModel(FileBlob model, FileBrowserViewModel browserVm)
        {
            Model = model;
            FileBrowserVm = browserVm;
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public void Extract()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = Model.FileName;
            if (dialog.ShowDialog().Value)
            {
                Model.ExportToFile(dialog.FileName);
            }
        }

        public void Delete()
        {
            ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, string.Format("File: {0}", Model.Name));
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                FileBrowserVm.Files.Remove(this);
            }
        }

        public void Rename()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, Model.Name);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Name = dialog.UserInput;
                Model.Save();
            }
        }

        public void Replace()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = Model.FileName;
            if (openFileDialog.ShowDialog().Value)
            {
                Model.FileName = Path.GetFileName(openFileDialog.FileName);
                string extension = Path.GetExtension(openFileDialog.FileName).ToLower();

                if (extension == ".png")
                    Model.FileType = FileBlob.FILE_TYPE_PNG;
                else if (extension == ".jpg" || extension == ".jpeg")
                    Model.FileType = FileBlob.FILE_TYPE_JPEG;
                else if (extension == ".dotx")
                    Model.FileType = FileBlob.FILE_TYPE_TEMPLATE;
                else
                    Model.FileType = FileBlob.FILE_TYPE_OTHER;

                Model.Save();
                Model.LoadFile(openFileDialog.FileName);
            }
        }
    }
}
