using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class FileBrowserViewModel : ViewModelBase
    {

        #region ICommands
        private ICommand _addFileCommand;
        public ICommand AddFileCommand
        {
            get
            {
                if (_addFileCommand == null)
                {
                    _addFileCommand = new RelayCommand(param => AddFile());
                }
                return _addFileCommand;
            }
        }

        private ICommand _moveUpCommand;
        public ICommand MoveItemUpCommand
        {
            get
            {
                if (_moveUpCommand == null)
                {
                    _moveUpCommand = new RelayCommand(param => MoveItemUp(), param => CanMoveItemUp());
                }
                return _moveUpCommand;
            }
        }

        private ICommand _moveDownCommand;
        public ICommand MoveItemDownCommand
        {
            get
            {
                if (_moveDownCommand == null)
                {
                    _moveDownCommand = new RelayCommand(param => MoveItemDown(), param => CanMoveItemDown());
                }
                return _moveDownCommand;
            }
        }
        #endregion

        public UniverseViewModel UniverseVm { get; private set; }
        public SortBySortIndexBindingList<FileBlobViewModel> Files { get; private set; }

        private FileBlobViewModel _selectedFile;
        public FileBlobViewModel SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                OnPropertyChanged("SelectedFile");
            }
        }

        public FileBrowserViewModel(UniverseViewModel universe)
        {
            UniverseVm = universe;
            Files = new SortBySortIndexBindingList<FileBlobViewModel>();
        }

        public void Load()
        {
            Files.Clear();

            var files = FileBlob.GetAllFileBlobs(UniverseVm.Model.Connection).Where(i => i.UniverseId == UniverseVm.Model.id);
            foreach (var f in files)
            {
                FileBlobViewModel vm = new FileBlobViewModel(f, this);
                Files.Add(vm);
            }
            if (Files.Count > 0)
                SelectedFile = Files[0];
            else
                SelectedFile = null;
        }

        public void AddFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog().Value)
            {
                NameItemDialog nameDialog = new NameItemDialog(DialogOwner, "New File");
                bool? dialogResult = nameDialog.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    FileBlob file = new FileBlob(UniverseVm.Model.Connection);
                    file.Name = nameDialog.UserInput;
                    file.FileName = Path.GetFileName(openFileDialog.FileName);
                    file.UniverseId = UniverseVm.Model.id;

                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();

                    if (extension == ".png")
                        file.FileType = FileBlob.FILE_TYPE_PNG;
                    else if (extension == ".jpg" || extension == ".jpeg")
                        file.FileType = FileBlob.FILE_TYPE_JPEG;
                    else if (extension == ".dotx")
                        file.FileType = FileBlob.FILE_TYPE_TEMPLATE;
                    else
                        file.FileType = FileBlob.FILE_TYPE_OTHER;

                    if (Files.Count > 0)
                        file.SortIndex = Files.Max(i => i.Model.SortIndex) + 1;
                    file.Create();
                    file.LoadFile(openFileDialog.FileName);

                    FileBlobViewModel vm = new FileBlobViewModel(file, this);
                    Files.Add(vm);
                }
            }
        }

        public bool CanMoveItemUp()
        {
            if (SelectedFile != null)
                return Files.CanMoveItemUp(SelectedFile);
            return false;
        }

        public bool CanMoveItemDown()
        {
            if (SelectedFile != null)
                return Files.CanMoveItemDown(SelectedFile);
            return false;
        }

        public void MoveItemUp()
        {
            Files.MoveItemUp(SelectedFile);
        }

        public void MoveItemDown()
        {
            Files.MoveItemDown(SelectedFile);
        }
    }
}
