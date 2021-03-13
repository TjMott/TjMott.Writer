using System;
using System.ComponentModel;
using WinDoc = System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model;
using TjMott.Writer.Model.SQLiteClasses;
using Docx = Xceed.Words.NET;
using System.Windows;
using System.Security.Cryptography;

namespace TjMott.Writer.ViewModel
{
    public class SceneViewModel : ViewModelBase, ISortable, IExportToWordDocument, IGetWordCount
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
        private ICommand _encryptCommand;
        public ICommand EncryptCommand
        {
            get
            {
                if (_encryptCommand == null)
                {
                    _encryptCommand = new RelayCommand(param => Encrypt(), param => CanEncrypt());
                }
                return _encryptCommand;
            }
        }

        private ICommand _decryptCommand;
        public ICommand DecryptCommand
        {
            get
            {
                if (_decryptCommand == null)
                {
                    _decryptCommand = new RelayCommand(param => Decrypt(), param => CanDecrypt());
                }
                return _decryptCommand;
            }
        }
        #endregion

        public Scene Model { get; private set; }
        public ChapterViewModel ChapterVm { get; set; }

        private SolidColorBrush _textColorBrush;
        public SolidColorBrush TextColorBrush
        {
            get 
            { 
                if (_textColorBrush == null)
                {
                    _textColorBrush = new SolidColorBrush(Color.FromArgb(Model.ColorA, Model.ColorR, Model.ColorG, Model.ColorB));
                }
                return _textColorBrush; 
            }
            set
            {
                _textColorBrush = value;
                Model.ColorA = _textColorBrush.Color.A;
                Model.ColorR = _textColorBrush.Color.R;
                Model.ColorG = _textColorBrush.Color.G;
                Model.ColorB = _textColorBrush.Color.B;
                Model.Save();
                OnPropertyChanged("TextColorBrush");
            }
        }

        public SceneViewModel(Scene model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;
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

        public void Delete()
        {
            ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, string.Format("Scene: {0}", Model.Name));
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                ChapterVm.DeleteScene(this);
            }
        }

        public void ExportToWord(Docx.DocX doc)
        {
            Model.SQLiteClasses.FlowDocument flowDoc = new Model.SQLiteClasses.FlowDocument(Model.Connection);
            flowDoc.id = Model.FlowDocumentId;
            flowDoc.Load();

            FlowDocumentViewModel vm = new FlowDocumentViewModel(flowDoc, DialogOwner);

            foreach (WinDoc.Block block in vm.Document.Blocks)
            {
                if (block is WinDoc.Paragraph)
                {
                    FlowDocumentExporter.AddParagraph((WinDoc.Paragraph)block, doc);
                }
            }
        }

        public void Encrypt()
        {
            FlowDocument fd = new FlowDocument(Model.Connection);
            fd.id = Model.FlowDocumentId;
            fd.Load();
            FlowDocumentViewModel vm = new FlowDocumentViewModel(fd, DialogOwner);
            fd.IsEncrypted = true;
            vm.GetAesPassword(true);
            vm.Save();
        }

        public bool CanEncrypt()
        {
            FlowDocument fd = new FlowDocument(Model.Connection);
            fd.id = Model.FlowDocumentId;
            fd.Load();
            return !fd.IsEncrypted;
        }

        public void Decrypt()
        {
            try
            {
                FlowDocument fd = new FlowDocument(Model.Connection);
                fd.id = Model.FlowDocumentId;
                fd.Load();
                FlowDocumentViewModel vm = new FlowDocumentViewModel(fd, DialogOwner);
                fd.IsEncrypted = false;
                fd.Save();
                vm.Save();
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Invalid password. Decryption operation canceled.", "Invalid password", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ApplicationException)
            {
                MessageBox.Show("Invalid password. Decryption operation canceled.", "Invalid password", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CanDecrypt()
        {
            FlowDocument fd = new FlowDocument(Model.Connection);
            fd.id = Model.FlowDocumentId;
            fd.Load();
            return fd.IsEncrypted;
        }
    }
}
