using System;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model;
using TjMott.Writer.Model.SQLiteClasses;
using Docx = Xceed.Words.NET;

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
        #endregion

        public Scene Model { get; private set; }
        public ChapterViewModel ChapterVm { get; set; }

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

            foreach (Block block in vm.Document.Blocks)
            {
                if (block is Paragraph)
                {
                    FlowDocumentExporter.AddParagraph((Paragraph)block, doc);
                }
            }
        }
    }
}
