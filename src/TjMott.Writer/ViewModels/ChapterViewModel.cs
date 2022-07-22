using Avalonia.Controls;
using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using TjMott.Writer.Models;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

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
        public async Task SaveAsync()
        {
            await Model.SaveAsync().ConfigureAwait(false);
        }
        #endregion

        #region ICommands
        public ReactiveCommand<Window, Unit> RenameCommand { get; }
        public ReactiveCommand<Window, Unit> DeleteCommand { get; }
        public ReactiveCommand<Window, Unit> CreateSceneCommand { get; }
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
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(RenameAsync);
            DeleteCommand = ReactiveCommand.CreateFromTask<Window>(DeleteAsync);
            CreateSceneCommand = ReactiveCommand.CreateFromTask<Window>(CreateScene);
            ShowPacingCommand = ReactiveCommand.Create(ShowPacing);
        }

        public async Task RenameAsync(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow(Model.Name);
            string result = await dialog.ShowDialog<string>(owner);
            if (result != null)
            {
                Model.Name = result;
                await Model.SaveAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(Window owner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Chapter: {0}", Model.Name));
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                await Model.DeleteAsync().ConfigureAwait(false);
                StoryVm.DeleteChapter(this);
            }
        }

        public async Task CreateScene(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow("New Scene");
            string result = await dialog.ShowDialog<string>(owner);
            if (!string.IsNullOrWhiteSpace(result))
            {
                Document doc = new Document(Model.Connection);
                doc.UniverseId = StoryVm.Model.UniverseId;
                doc.WordCount = 0;
                doc.PlainText = "";
                doc.Json = "";
                await doc.CreateAsync().ConfigureAwait(false);

                Scene scene = new Scene(Model.Connection);
                scene.ChapterId = Model.id;
                scene.Name = result;
                if (Scenes.Count == 0)
                    scene.SortIndex = 0;
                else
                    scene.SortIndex = Scenes.Max(i => i.Model.SortIndex) + 1;
                scene.DocumentId = doc.id;
                await scene.CreateAsync().ConfigureAwait(false);
                SceneViewModel sceneVm = new SceneViewModel(scene);
                sceneVm.ChapterVm = this;
                Scenes.Add(sceneVm);
            }
        }

        public void DeleteScene(SceneViewModel scene)
        {
            Scenes.Remove(scene);
        }

        public async void UpdateSceneSortIndices()
        {
            for (int i = 0; i < Scenes.Count; i++)
            {
                Scenes[i].Model.SortIndex = i;
                await Scenes[i].SaveAsync().ConfigureAwait(false);
            }
        }

        public void ShowPacing()
        {
            var chapters = new List<ChapterViewModel>();
            chapters.Add(this);
            PacingWindow.ShowPacingWindow(chapters, string.Format("Pacing: {0}", Model.Name), "chapter_" + Model.id);
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
