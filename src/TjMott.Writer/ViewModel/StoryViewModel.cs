using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model.SQLiteClasses;
using Docx = Xceed.Words.NET;

namespace TjMott.Writer.ViewModel
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
        public void Save()
        {
            Model.Save();
        }
        #endregion

        #region ICommands
        private ICommand _editPropertiesCommand;
        public ICommand EditPropertiesCommand
        {
            get
            {
                if (_editPropertiesCommand == null)
                {
                    _editPropertiesCommand = new RelayCommand(param => EditProperties());
                }
                return _editPropertiesCommand;
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

        private ICommand _createChapterCommand;
        public ICommand CreateChapterCommand
        {
            get
            {
                if (_createChapterCommand == null)
                {
                    _createChapterCommand = new RelayCommand(param => CreateChapter());
                }
                return _createChapterCommand;
            }
        }
        #endregion

        public Story Model { get; private set; }
        public UniverseViewModel UniverseVm { get; set; }
        public SortBySortIndexBindingList<ChapterViewModel> Chapters { get; private set; }
        public StoryViewModel(Story model)
        {
            Model = model;
            Chapters = new SortBySortIndexBindingList<ChapterViewModel>();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public void EditProperties()
        {
            StoryPropertiesDialog dialog = new StoryPropertiesDialog(DialogOwner, Model, UniverseVm.Categories);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Save();
                UniverseVm.UpdateStoryInTree(this);
            }
        }

        public void Delete()
        {
            ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, string.Format("Story: {0}", Model.Name));
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                UniverseVm.DeleteSubItem(this);
            }
        }

        public void CreateChapter()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, "New Chapter");
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Chapter chapter = new Chapter(Model.Connection);
                chapter.StoryId = Model.id;
                chapter.Name = dialog.UserInput;
                if (Chapters.Count == 0)
                    chapter.SortIndex = 0;
                else
                    chapter.SortIndex = Chapters.Max(i => i.Model.SortIndex) + 1;
                chapter.Create();
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

        public void UpdateChapterSortIndices()
        {
            for (int i = 0; i < Chapters.Count; i++)
            {
                Chapters[i].Model.SortIndex = i;
                Chapters[i].Save();
            }
        }

        public void ExportToWord(Docx.DocX doc)
        {
            // Format title page of the document.
            Xceed.Document.NET.Paragraph p = doc.InsertParagraph();
            p.Append("\n\n" + Model.Name);
            p.StyleName = "Title";

            if (!string.IsNullOrWhiteSpace(Model.Subtitle))
            {
                p = doc.InsertParagraph();
                p.Append(Model.Subtitle);
                p.StyleName = "Subtitle";
            }
            if (!string.IsNullOrWhiteSpace(Model.Author))
            {
                p = doc.InsertParagraph();
                p.Append("\n\n" + Model.Author);
                p.StyleName = "Subtitle";
            }
            p.InsertPageBreakAfterSelf();

            // Insert copyright page.
            p = doc.InsertParagraph();
            p.Append("This is a work of fiction. All characters and events portrayed in this book are fictional, and any resemblance to real people or incidents is purely coincidental.\n");
            p.StyleName = "Copyright";

            p = doc.InsertParagraph();
            p.Append(string.Format("Copyright © {0} by {1}. All rights reserved.\n", DateTime.Now.Year, Model.Author));
            p.StyleName = "Copyright";


            if (!string.IsNullOrWhiteSpace(Model.ISBN))
            {
                p = doc.InsertParagraph();
                p.Append(string.Format("ISBN: {0}\n", Model.ISBN));
                p.StyleName = "Copyright";
            }

            if (!string.IsNullOrWhiteSpace(Model.Edition))
            {
                p = doc.InsertParagraph();
                p.Append(Model.Edition + "\n");
                p.StyleName = "Copyright";
            }


            p.InsertPageBreakAfterSelf();

            // Insert eBook table of contents.
            Xceed.Document.NET.TableOfContents toc = doc.InsertTableOfContents("Table of Contents", Xceed.Document.NET.TableOfContentsSwitches.H | Xceed.Document.NET.TableOfContentsSwitches.N | Xceed.Document.NET.TableOfContentsSwitches.Z | Xceed.Document.NET.TableOfContentsSwitches.U);
            doc.InsertSectionPageBreak();

            for (int i = 0; i < Chapters.Count; i++)
            {
                ChapterViewModel chapter = Chapters[i];
                chapter.ExportToWord(doc);
                doc.InsertSectionPageBreak();
                doc.Paragraphs.Last().InsertPageBreakAfterSelf();
            }
        }
    }
}
