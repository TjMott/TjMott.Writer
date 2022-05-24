using System;
using System.ComponentModel;
using System.Security.Cryptography;
using TjMott.Writer.Models;
using TjMott.Writer.Models.SQLiteClasses;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels
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
        public ReactiveCommand<Unit, Unit> RenameCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> EncryptCommand { get; }
        public ReactiveCommand<Unit, Unit> DecryptCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveToChapterCommand { get; }
        #endregion

        public long GetWordCount()
        {
            Document fd = new Document(Model.Connection);
            fd.id = Model.DocumentId;
            fd.Load();
            if (fd.IsEncrypted)
                return 0;
            else
                return fd.WordCount;
        }
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

        private bool _isEncrypted = false;
        public bool IsEncrypted
        {
            get { return _isEncrypted; }
            private set
            {
                _isEncrypted = value;
                OnPropertyChanged("IsEncrypted");
            }
        }

        public SceneViewModel(Scene model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;

            RenameCommand = ReactiveCommand.Create(Rename);
            DeleteCommand = ReactiveCommand.Create(Delete);
            EncryptCommand = ReactiveCommand.Create(Encrypt);
            DecryptCommand = ReactiveCommand.Create(Decrypt);
            MoveToChapterCommand = ReactiveCommand.Create(MoveToChapter);
            
            // Inits the IsEncrypted field.
            CanDecrypt();
        }

        public async void Rename()
        {
            NameItemWindow dialog = new NameItemWindow(Model.Name);
            string result = await dialog.ShowDialog<string>(MainWindow);
            if (result != null)
            {
                Model.Name = result;
                Model.Save();
            }
        }

        public async void Delete()
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Scene: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(MainWindow);
            if (result)
            {
                Model.Delete();
                ChapterVm.DeleteScene(this);
            }
        }

        public void MoveToChapter()
        {
            MoveSceneToChapterWindow d = new MoveSceneToChapterWindow(this);
            d.ShowDialog<bool>(MainWindow);
        }

        /*public void ExportToWord(Docx.DocX doc)
        {
            FlowDocument flowDoc = new FlowDocument(Model.Connection);
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
        }*/

        public void Encrypt()
        {
            /*FlowDocument fd = new FlowDocument(Model.Connection);
            fd.id = Model.FlowDocumentId;
            fd.Load();
            FlowDocumentViewModel vm = new FlowDocumentViewModel(fd, DialogOwner);
            fd.IsEncrypted = true;
            vm.GetAesPassword(true);
            vm.Save();
            IsEncrypted = true;*/
        }

        public bool CanEncrypt()
        {
            Document fd = new Document(Model.Connection);
            fd.id = Model.DocumentId;
            fd.Load();
            IsEncrypted = fd.IsEncrypted;
            return !fd.IsEncrypted;
        }

        public void Decrypt()
        {
            /*try
            {
                FlowDocument fd = new FlowDocument(Model.Connection);
                fd.id = Model.FlowDocumentId;
                fd.Load();
                FlowDocumentViewModel vm = new FlowDocumentViewModel(fd, DialogOwner);
                fd.IsEncrypted = false;
                fd.Save();
                vm.Save();
                IsEncrypted = false;
            }
            catch (CryptographicException)
            {
                MessageBox.Show("Invalid password. Decryption operation canceled.", "Invalid password", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ApplicationException)
            {
                MessageBox.Show("Invalid password. Decryption operation canceled.", "Invalid password", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
        }

        public bool CanDecrypt()
        {
            Document fd = new Document(Model.Connection);
            fd.id = Model.DocumentId;
            fd.Load();
            IsEncrypted = fd.IsEncrypted;
            return fd.IsEncrypted;
        }
    }
}
