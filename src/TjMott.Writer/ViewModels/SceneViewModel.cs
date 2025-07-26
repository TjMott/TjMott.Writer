using Avalonia.Controls;
using Avalonia.Media;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TjMott.Writer.Models;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;
using Xceed.Document.NET;
using Xceed.Words.NET;
using static TjMott.Writer.ViewModels.IExportToWordDocument;
using Document = TjMott.Writer.Models.SQLiteClasses.Document;

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

        public async Task<long> GetWordCountAsync()
        {
            Document doc = new Document(Model.Connection);
            doc.id = Model.DocumentId;
            await doc.LoadAsync();
            if (doc.IsEncrypted)
                return 0;
            else
                return doc.WordCount;
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
                await Model.SaveAsync();
            }
        }

        public async Task Delete(Window owner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Scene: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.DeleteAsync();
                ChapterVm.DeleteScene(this);
            }
        }

        public async Task MoveToChapter(Window owner)
        {
            MoveSceneToChapterWindow d = new MoveSceneToChapterWindow(this);
            await d.ShowDialog<bool>(owner);
        }

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
                    JObject attributes = op["attributes"] as JObject;
                    DocumentExporter.ApplyFormatting(f, para, attributes);

                    // TODO: List formatting.
                    // Quill likes to not put the list attribute on the first list item,
                    // so this isn't really usable yet.
                    if (attributes.ContainsKey("list"))
                    {
                        string list = attributes["list"].Value<string>();
                        if (list == "ordered")
                        {

                        }
                        else if (list == "bullet")
                        {

                        }
                        // Not really sure how to do this.
                    }
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
