using ReactiveUI;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using TjMott.Writer.Models;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.ViewModels
{
    public class ChapterViewModel : ViewModelBase, ISortable, IExportToWordDocument, IGetWordCount
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
        public ReactiveCommand<Unit, Unit> CreateSceneCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowPacingCommand { get; }
        #endregion

        public long GetWordCount()
        {
            return Scenes.Sum(i => i.GetWordCount());
        }
        public Chapter Model { get; private set; }
        public StoryViewModel StoryVm { get; set; }
        public SortBySortIndexBindingList<SceneViewModel> Scenes { get; private set; }

        public ChapterViewModel(Chapter model)
        {
            Model = model;
            Scenes = new SortBySortIndexBindingList<SceneViewModel>();
            Model.PropertyChanged += Model_PropertyChanged;
            RenameCommand = ReactiveCommand.Create(Rename);
            DeleteCommand = ReactiveCommand.Create(Delete);
            CreateSceneCommand = ReactiveCommand.Create(CreateScene);
            ShowPacingCommand = ReactiveCommand.Create(ShowPacing);
        }

        public void Rename()
        {
            /*NameItemDialog dialog = new NameItemDialog(DialogOwner, Model.Name);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Name = dialog.UserInput;
                Model.Save();
            }*/
        }

        public void Delete()
        {
            /*ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, string.Format("Chapter: {0}", Model.Name));
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                StoryVm.DeleteChapter(this);
            }*/
        }

        public void CreateScene()
        {
            /*NameItemDialog dialog = new NameItemDialog(DialogOwner, "New Scene");
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                FlowDocument doc = new FlowDocument(Model.Connection);
                doc.UniverseId = StoryVm.Model.UniverseId;
                doc.WordCount = 0;
                doc.PlainText = "";
                doc.Xml = FlowDocumentViewModel.GetEmptyFlowDocXml();
                doc.Create();

                Scene scene = new Scene(Model.Connection);
                scene.ChapterId = Model.id;
                scene.Name = dialog.UserInput;
                if (Scenes.Count == 0)
                    scene.SortIndex = 0;
                else
                    scene.SortIndex = Scenes.Max(i => i.Model.SortIndex) + 1;
                scene.FlowDocumentId = doc.id;
                scene.Create();
                SceneViewModel sceneVm = new SceneViewModel(scene);
                sceneVm.ChapterVm = this;
                Scenes.Add(sceneVm);
            }*/
        }

        public void DeleteScene(SceneViewModel scene)
        {
            Scenes.Remove(scene);
        }

        public void UpdateSceneSortIndices()
        {
            for (int i = 0; i < Scenes.Count; i++)
            {
                Scenes[i].Model.SortIndex = i;
                Scenes[i].Save();
            }
        }

        public void ShowPacing()
        {
            /*var chapters = new List<ChapterViewModel>();
            chapters.Add(this);
            PacingWindow wnd = new PacingWindow(chapters);
            wnd.Show();*/
        }

        /*public void ExportToWord(Docx.DocX doc)
        {
            // Skip encrypted scenes.
            var scenes = Scenes.Where(i => !i.IsEncrypted).OrderBy(i => i.SortIndex).ToList();

            if (scenes.Count > 0)
            {
                // Export chapter name.
                Xceed.Document.NET.Paragraph chapterHeader = doc.InsertParagraph();
                chapterHeader.StyleId = "Heading1";
                chapterHeader.Append(Model.Name);

                doc.InsertParagraph();
                doc.InsertParagraph();
                doc.InsertParagraph();

                for (int i = 0; i < scenes.Count; i++)
                {
                    SceneViewModel scene = scenes[i];
                    scene.ExportToWord(doc);
                    if (i < Scenes.Count - 1)
                    {
                        Xceed.Document.NET.Paragraph p = doc.InsertParagraph();
                        p.StyleId = "SceneBreak";
                        p.Append("\n*\t\t*\t\t*\n");
                    }
                }
                doc.Paragraphs.Last().InsertPageBreakAfterSelf();
            }
        }*/
    }
}
