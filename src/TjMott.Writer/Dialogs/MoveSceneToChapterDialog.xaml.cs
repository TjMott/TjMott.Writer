using System;
using System.Windows;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Dialogs
{
    /// <summary>
    /// Interaction logic for MoveSceneToChapterDialog.xaml
    /// </summary>
    public partial class MoveSceneToChapterDialog : Window
    {
        public SceneViewModel SceneToMove { get; set; }
        public MoveSceneToChapterDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chapterListBox.ItemsSource = SceneToMove.ChapterVm.StoryVm.Chapters;
            chapterListBox.SelectedItem = SceneToMove.ChapterVm;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            ChapterViewModel originalChapter = SceneToMove.ChapterVm;
            ChapterViewModel newChapter = (ChapterViewModel)chapterListBox.SelectedItem;

            if (originalChapter != newChapter)
            {
                originalChapter.Scenes.Remove(SceneToMove);
                newChapter.Scenes.Add(SceneToMove);
                SceneToMove.ChapterVm = newChapter;
                SceneToMove.Model.ChapterId = newChapter.Model.id;
                SceneToMove.Model.Save();
            }
            DialogResult = true;
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
