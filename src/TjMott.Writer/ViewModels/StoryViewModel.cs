using System;
using System.ComponentModel;
using System.Linq;
using TjMott.Writer.Models.SQLiteClasses;
using ReactiveUI;
using System.Reactive;
using TjMott.Writer.Views;
using System.Threading.Tasks;
using Avalonia.Controls;

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
            await Model.SaveAsync().ConfigureAwait(false);
        }
        #endregion

        #region ICommands
        public ReactiveCommand<Window, Unit> EditPropertiesCommand { get; }

        public ReactiveCommand<Window, Unit> DeleteCommand { get; }

        public ReactiveCommand<Window, Unit> CreateChapterCommand { get; }

        public ReactiveCommand<Window, Unit> EditCopyrightPageCommand { get; }

        public ReactiveCommand<Unit, Unit> ShowPacingCommand { get; }
        #endregion

        public long GetWordCount()
        {
            return Chapters.Sum(i => i.GetWordCount());
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

            EditPropertiesCommand = ReactiveCommand.Create<Window>(EditProperties);
            DeleteCommand = ReactiveCommand.Create<Window>(Delete);
            CreateChapterCommand = ReactiveCommand.Create<Window>(CreateChapter);
            EditCopyrightPageCommand = ReactiveCommand.Create<Window>(EditCopyrightPage);
            ShowPacingCommand = ReactiveCommand.Create(ShowPacing);
        }

        public async void EditProperties(Window owner)
        {
            EditStoryPropertiesWindow dialog = new EditStoryPropertiesWindow();
            dialog.DataContext = new EditStoryPropertiesWindowViewModel(Model, UniverseVm.Categories);
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.SaveAsync().ConfigureAwait(false);
                UniverseVm.UpdateStoryInTree(this);
            }
        }

        public async void EditCopyrightPage(Window owner)
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

        public async void Delete(Window owner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Story: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.DeleteAsync().ConfigureAwait(false);
                UniverseVm.DeleteSubItem(this);
            }
        }

        public async void CreateChapter(Window owner)
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
                await chapter.CreateAsync().ConfigureAwait(false);
                ChapterViewModel chapterVm = new ChapterViewModel(chapter);
                chapterVm.StoryVm = this;
                Chapters.Add(chapterVm);
            }
        }

        public void DeleteChapter(ChapterViewModel chapter)
        {
            Chapters.Remove(chapter);
            UpdateChapterSortIndices();
        }

        public async void UpdateChapterSortIndices()
        {
            for (int i = 0; i < Chapters.Count; i++)
            {
                Chapters[i].Model.SortIndex = i;
                await Chapters[i].SaveAsync().ConfigureAwait(false);
            }
        }

        public void ShowPacing()
        {
            PacingWindow.ShowPacingWindow(Chapters, string.Format("Pacing: {0}", Model.Name), "story_" + Model.id);
        }

        /*public void ExportToWord(Docx.DocX doc)
        {
            // Format title page of the document.
            Xceed.Document.NET.Paragraph p = doc.InsertParagraph();
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
            if (Model.FlowDocumentId.HasValue)
            {
                FlowDocument copyrightFd = new FlowDocument(Model.Connection);
                copyrightFd.id = Model.FlowDocumentId.Value;
                copyrightFd.Load();

                FlowDocumentViewModel copyrightVm = new FlowDocumentViewModel(copyrightFd, DialogOwner);

                foreach (WinDoc.Block block in copyrightVm.Document.Blocks)
                {
                    if (block is WinDoc.Paragraph)
                    {
                        FlowDocumentExporter.AddParagraph((WinDoc.Paragraph)block, doc);
                    }
                }

                p.InsertPageBreakAfterSelf();
            }

            // Insert eBook table of contents.

            // This doesn't make sense to me. Xceed's documentation says "A key-value dictionary where the key is a TableOfContentSwitches 
            // and the value is the parameter of the switch."
            // I have no idea what the string values are supposed to be. Empty strings seem to work.
            Dictionary<Xceed.Document.NET.TableOfContentsSwitches, string> tocParams = new Dictionary<Xceed.Document.NET.TableOfContentsSwitches, string>();
            tocParams[Xceed.Document.NET.TableOfContentsSwitches.H] = ""; // TOC entries are clickable hyperlinks.
            tocParams[Xceed.Document.NET.TableOfContentsSwitches.N] = ""; // Omits page numbers.
            tocParams[Xceed.Document.NET.TableOfContentsSwitches.Z] = ""; // Hides tab leader and page numbers...?
            tocParams[Xceed.Document.NET.TableOfContentsSwitches.U] = ""; // Uses the applied paragraph outline level...?
            Xceed.Document.NET.TableOfContents toc = doc.InsertTableOfContents("Table of Contents", tocParams);
            doc.Paragraphs.Last().InsertPageBreakAfterSelf();

            for (int i = 0; i < Chapters.Count; i++)
            {
                ChapterViewModel chapter = Chapters[i];
                chapter.ExportToWord(doc);
            }
        }*/
    }
}
