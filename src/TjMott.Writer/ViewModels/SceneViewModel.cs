using System;
using System.ComponentModel;
using System.Security.Cryptography;
using TjMott.Writer.Models;
using TjMott.Writer.Models.SQLiteClasses;
using Avalonia.Media;
using ReactiveUI;
using System.Reactive;
using TjMott.Writer.Views;
using System.Threading.Tasks;
using Avalonia.Controls;
using Xceed.Words.NET;
using System.Threading;
using Newtonsoft.Json.Linq;
using Xceed.Document.NET;
using Document = TjMott.Writer.Models.SQLiteClasses.Document;
using static TjMott.Writer.ViewModels.IExportToWordDocument;
using System.Linq;

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
        public async Task SaveAsync()
        {
            await Model.SaveAsync();
        }
        #endregion

        #region ICommands
        public ReactiveCommand<Window, Unit> RenameCommand { get; }
        public ReactiveCommand<Window, Unit> DeleteCommand { get; }
        public ReactiveCommand<Window, Unit> MoveToChapterCommand { get; }
        #endregion

        public long GetWordCount()
        {
            Document fd = new Document(Model.Connection);
            fd.id = Model.DocumentId;
            fd.LoadAsync().Wait();
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
                Model.SaveAsync().Wait();
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

            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);
            DeleteCommand = ReactiveCommand.CreateFromTask<Window>(Delete);
            MoveToChapterCommand = ReactiveCommand.CreateFromTask<Window>(MoveToChapter);
            
            // Inits the IsEncrypted field.
            CanDecrypt();
        }

        public async Task Rename(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow(Model.Name);
            string result = await dialog.ShowDialog<string>(owner);
            if (result != null)
            {
                Model.Name = result;
                await Model.SaveAsync().ConfigureAwait(false);
            }
        }

        public async Task Delete(Window owner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Scene: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.DeleteAsync().ConfigureAwait(false);
                ChapterVm.DeleteScene(this);
            }
        }

        public async Task MoveToChapter(Window owner)
        {
            MoveSceneToChapterWindow d = new MoveSceneToChapterWindow(this);
            await d.ShowDialog<bool>(owner);
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

        public bool CanEncrypt()
        {
            Document fd = new Document(Model.Connection);
            fd.id = Model.DocumentId;
            fd.LoadAsync().Wait();
            IsEncrypted = fd.IsEncrypted;
            return !fd.IsEncrypted;
        }

        public bool CanDecrypt()
        {
            Document fd = new Document(Model.Connection);
            fd.id = Model.DocumentId;
            fd.LoadAsync().Wait();
            IsEncrypted = fd.IsEncrypted;
            return fd.IsEncrypted;
        }

        public void ShowJson()
        {
            Document fd = new Document(Model.Connection);
            fd.id = Model.DocumentId;
            DocumentJsonWindow d = new DocumentJsonWindow(fd);
            d.Show();
        }

        public async Task ExportToWordAsync(DocX doc, ExportOptions exportOptions, CancellationToken cancelToken)
        {
            Document dbDoc = new Document(Model.Connection);
            dbDoc.id = Model.DocumentId;
            await dbDoc.LoadAsync();
            if (dbDoc.IsEncrypted)
            {
                if (!exportOptions.ExportEncryptedDocs)
                {
                    return;
                }
                else
                {
                    // TODO
                    return;
                }
            }
            JObject json = dbDoc.GetJObject();

            Paragraph para = doc.InsertParagraph();
                
            foreach (JObject op in json["ops"])
            {
                cancelToken.ThrowIfCancellationRequested();
                Formatting f = new Formatting();

                // Apply default styles.
                DocumentExporter.ApplyFormatting(f, para, exportOptions.DefaultOpAttributes);

                // Override styles with this op's styles.
                if (op.ContainsKey("attributes"))
                {
                    DocumentExporter.ApplyFormatting(f, para, op["attributes"] as JObject);
                }
                string text = op["insert"].Value<string>().Replace("\r", "");
                string[] parts = text.Split("\n");
                for (int i = 0; i < parts.Length; i++)
                {
                    para.Append(parts[i], f);
                    if (i < parts.Length - 1)
                        para = doc.InsertParagraph();
                }

                exportOptions.OpsProcessed++;
            }

            if (dbDoc.IsEncrypted)
                dbDoc.Lock();
        }

        public async Task<int> GetOpsCount(Window dialogOwner, ExportOptions exportOptions, CancellationToken cancelToken)
        {
            cancelToken.ThrowIfCancellationRequested();
            Document dbDoc = new Document(Model.Connection);
            dbDoc.id = Model.DocumentId;
            await dbDoc.LoadAsync();
            if (dbDoc.IsEncrypted)
            {
                if (!exportOptions.ExportEncryptedDocs)
                {
                    return 0;
                }
                else
                {
                    // TODO
                    return 0;
                }
            }

            JObject json = dbDoc.GetJObject();

            if (dbDoc.IsEncrypted)
                dbDoc.Lock();

            return json["ops"].Count();
        }
    }
}
