using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class MoveSceneToChapterWindow : Window
    {
        private SceneViewModel _scene;
        public MoveSceneToChapterWindow()
        {
            InitializeComponent();
        }

        public MoveSceneToChapterWindow(SceneViewModel scene)
        {
            _scene = scene;
            InitializeComponent();
            Activated += MoveSceneToChapterWindow_Activated;
        }
        private void MoveSceneToChapterWindow_Activated(object sender, System.EventArgs e)
        {
            Activated -= MoveSceneToChapterWindow_Activated;
            //this.FindControl<ListBox>("chapterListBox").Items = _scene.ChapterVm.StoryVm.Chapters;
            //this.FindControl<ListBox>("chapterListBox").SelectedItem = _scene.ChapterVm;
        }

        private async void okButton_Click(object sender, RoutedEventArgs e)
        {
            ChapterViewModel originalChapter = _scene.ChapterVm;
            ChapterViewModel newChapter = (ChapterViewModel)chapterListBox.SelectedItem;

            if (originalChapter != newChapter)
            {
                originalChapter.Scenes.Remove(_scene);
                newChapter.Scenes.AddToEnd(_scene);
                _scene.ChapterVm = newChapter;
                _scene.Model.ChapterId = newChapter.Model.id;
                await _scene.Model.SaveAsync();
            }
            Close(true);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
