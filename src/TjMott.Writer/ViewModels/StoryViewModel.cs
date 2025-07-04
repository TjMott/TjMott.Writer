using System;
using System.ComponentModel;
using System.Linq;
using TjMott.Writer.Models.SQLiteClasses;
using ReactiveUI;
using System.Reactive;
using TjMott.Writer.Views;
using System.Threading.Tasks;
using Avalonia.Controls;
using Xceed.Words.NET;
using System.Threading;
using Newtonsoft.Json.Linq;
using static TjMott.Writer.ViewModels.IExportToWordDocument;
using Xceed.Document.NET;
using Document = TjMott.Writer.Models.SQLiteClasses.Document;
using System.Collections.Generic;

namespace TjMott.Writer.ViewModels
{
    public class StoryViewModel : ViewModelBase, IUniverseSubItem, IExportToWordDocument, IGetWordCount
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
        public ReactiveCommand<Window, Unit> EditPropertiesCommand { get; }

        public ReactiveCommand<Window, Unit> DeleteCommand { get; }

        public ReactiveCommand<Window, Unit> CreateChapterCommand { get; }

        public ReactiveCommand<Window, Unit> EditCopyrightPageCommand { get; }

        public ReactiveCommand<Unit, Unit> ShowPacingCommand { get; }
        #endregion

        public async Task<long> GetWordCountAsync()
        {
            if (Chapters.Count == 0)
                await LoadChapters();
            long wordCount = 0;

            foreach (var chapter in Chapters)
            {
                wordCount += await chapter.GetWordCountAsync();
            }
            return wordCount;
        }

        public Story Model { get; private set; }
        public UniverseViewModel UniverseVm { get; set; }
        public CategoryViewModel Category
        {
            get
            {
                if (Model.CategoryId.HasValue)
                {
                    return UniverseVm.Categories.SingleOrDefault(i => i.Model.id == Model.CategoryId.Value);
                }
                return null;
            }
        }
        public SortBySortIndexBindingList<ChapterViewModel> Chapters { get; private set; }
        public StoryViewModel(Story model)
        {
            Model = model;
            Chapters = new SortBySortIndexBindingList<ChapterViewModel>();
            Model.PropertyChanged += Model_PropertyChanged;

            EditPropertiesCommand = ReactiveCommand.CreateFromTask<Window>(EditProperties);
            DeleteCommand = ReactiveCommand.CreateFromTask<Window>(Delete);
            CreateChapterCommand = ReactiveCommand.CreateFromTask<Window>(CreateChapter);
            EditCopyrightPageCommand = ReactiveCommand.CreateFromTask<Window>(EditCopyrightPage);
            ShowPacingCommand = ReactiveCommand.Create(ShowPacing);
        }

        public async Task EditProperties(Window owner)
        {
            EditStoryPropertiesWindow dialog = new EditStoryPropertiesWindow();
            dialog.DataContext = new EditStoryPropertiesWindowViewModel(Model, UniverseVm.Categories);
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.SaveAsync();
                UniverseVm.UpdateStoryInTree(this);
            }
        }

        public async Task EditCopyrightPage(Window owner)
        {
            if (!Model.CopyrightPageId.HasValue)
            {
                Document doc = new Document(Model.Connection);
                doc.UniverseId = Model.UniverseId;
                doc.IsEncrypted = false;
                doc.WordCount = 0;
                doc.PlainText = "";
                doc.Json = "";
                doc.DocumentType = "CopyrightPage";
                await doc.CreateAsync();

                Model.CopyrightPageId = doc.id;
                await Model.SaveAsync();
            }
            if (Model.CopyrightPageId.HasValue)
            {
                EditCopyrightPageWindow.ShowEditorWindow(this);
            }
        }

        public async Task LoadChapters()
        {
            Chapters.Clear();
            var chapterModels = await Chapter.LoadForStory(Model.id, Model.Connection);
            foreach (var chapter in chapterModels)
            {
                ChapterViewModel cvm = new ChapterViewModel(chapter);
                cvm.StoryVm = this;

                // Load scenes.
                var scenes = await Scene.LoadForChapter(chapter.id, Model.Connection);
                foreach (var scene in scenes)
                {
                    SceneViewModel svm = new SceneViewModel(scene);
                    svm.ChapterVm = cvm;
                    cvm.Scenes.Add(svm);
                }

                Chapters.Add(cvm);
            }
        }

        public void UnloadChapters()
        {
            Chapters.Clear();
        }

        public async Task Delete(Window owner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Story: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.DeleteAsync();
                await UniverseVm.DeleteSubItem(this);
            }
        }

        public async Task CreateChapter(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow("New Chapter");
            string result = await dialog.ShowDialog<string>(owner);
            if (!string.IsNullOrWhiteSpace(result))
            {
                Chapter chapter = new Chapter(Model.Connection);
                chapter.StoryId = Model.id;
                chapter.Name = result;
                if (Chapters.Count == 0)
                    chapter.SortIndex = 0;
                else
                    chapter.SortIndex = Chapters.Max(i => i.Model.SortIndex) + 1;
                await chapter.CreateAsync();
                ChapterViewModel chapterVm = new ChapterViewModel(chapter);
                chapterVm.StoryVm = this;
                Chapters.Add(chapterVm);
            }
        }

        public void DeleteChapter(ChapterViewModel chapter)
        {
            Chapters.Remove(chapter);
        }

        public void ShowPacing()
        {
            PacingWindow.ShowPacingWindow(Chapters, string.Format("Pacing: {0}", Model.Name), "story_" + Model.id);
        }

        public async Task ExportToWordAsync(DocX doc, ExportOptions exportOptions, CancellationToken cancelToken)
        {
            // Format title page.
            Paragraph p = doc.InsertParagraph();
            p.Append("\n\n" + Model.Name);
            p.StyleId = "Title";

            if (!string.IsNullOrWhiteSpace(Model.Subtitle))
            {
                p = doc.InsertParagraph();
                p.Append(Model.Subtitle);
                p.StyleId = "Subtitle";
            }
            if (!string.IsNullOrWhiteSpace(Model.Author))
            {
                p = doc.InsertParagraph();
                p.Append("\n\n" + Model.Author);
                p.StyleId = "Subtitle";
            }
            p.InsertPageBreakAfterSelf();

            // Insert copyright page.
            if (Model.CopyrightPageId.HasValue)
            {
                Document copyrightPage = new Document(Model.Connection);
                copyrightPage.id = Model.CopyrightPageId.Value;
                await copyrightPage.LoadAsync();

                JObject copyrightOps = copyrightPage.GetJObject();

                foreach (JObject op in copyrightOps["ops"])
                {
                    cancelToken.ThrowIfCancellationRequested();
                    Formatting f = new Formatting();
                    DocumentExporter.ApplyFormatting(f, null, exportOptions.DefaultOpAttributes);

                    p = doc.InsertParagraph();

                    // Override styles with this op's styles.
                    if (op.ContainsKey("attributes"))
                    {
                        DocumentExporter.ApplyFormatting(f, p, op["attributes"] as JObject);
                    }
                    string text = op["insert"].Value<string>().Replace("\r", "");
                    string[] parts = text.Split("\n");
                    for (int i = 0; i < parts.Length; i++)
                    {
                        p.Append(parts[i], f);
                        if (i < parts.Length - 1)
                            p = doc.InsertParagraph();
                    }

                }
            }

            // Insert ebook table of contents.

            // This doesn't make sense to me. Xceed's documentation says "A key-value dictionary where the key is a TableOfContentSwitches 
            // and the value is the parameter of the switch."
            // I have no idea what the string values are supposed to be. Empty strings seem to work.
            Dictionary<TableOfContentsSwitches, string> tocParams = new Dictionary<TableOfContentsSwitches, string>();
            tocParams[TableOfContentsSwitches.H] = ""; // TOC entries are clickable hyperlinks.
            tocParams[TableOfContentsSwitches.N] = ""; // Omits page numbers.
            tocParams[TableOfContentsSwitches.Z] = ""; // Hides tab leader and page numbers...?
            tocParams[TableOfContentsSwitches.U] = ""; // Uses the applied paragraph outline level...?
            TableOfContents toc = doc.InsertTableOfContents("Table of Contents", tocParams);
           
            doc.InsertParagraph();
            doc.Paragraphs.Last().InsertPageBreakAfterSelf();

            // Now export chapters.
            foreach (var chapter in Chapters.OrderBy(i => i.SortIndex).ToList())
            {
                cancelToken.ThrowIfCancellationRequested();
                await chapter.ExportToWordAsync(doc, exportOptions, cancelToken);
            }
        }

        public async Task<int> GetOpsCount(Window dialogOwner, ExportOptions exportOptions, CancellationToken cancelToken)
        {
            int count = 0;
            foreach (var chapter in Chapters)
                count += await chapter.GetOpsCount(dialogOwner, exportOptions, cancelToken);
            return count;
        }
    }
}
