using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TjMott.Writer.ViewModel;

namespace TjMott.Writer.Windows
{
    /// <summary>
    /// Interaction logic for PacingWindow.xaml
    /// </summary>
    public partial class PacingWindow : Window
    {
        private int colorIndex = -1;
        private List<Color> colors = new List<Color>()
        {
            Colors.LightBlue,
            Colors.LightGoldenrodYellow,
            Colors.LightGray,
            Colors.LightGreen,
            Colors.LightSalmon,
            Colors.LightSkyBlue,
            Colors.LightPink
        };

        private IEnumerable<ChapterViewModel> _chapters;
        public PacingWindow(IEnumerable<ChapterViewModel> chapters)
        {
            InitializeComponent();
            _chapters = chapters;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            doRender();
        }

        private void doRender()
        {
            chapterGrid.Children.Clear();
            chapterGrid.ColumnDefinitions.Clear();
            int gridCol = 0;
            double totalWordCount = 0;

            foreach (var chap in _chapters)
            {
                totalWordCount += chap.GetWordCount();
            }

            foreach (var chap in _chapters)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(chap.GetWordCount(), GridUnitType.Star);
                chapterGrid.ColumnDefinitions.Add(cd);

                Border b = new Border();
                chapterGrid.Children.Add(b);
                Grid.SetColumn(b, gridCol);
                b.BorderBrush = new SolidColorBrush(Colors.Black);
                b.BorderThickness = new Thickness(2);

                Grid g = new Grid();
                b.Child = g;
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                TextBlock t = new TextBlock();
                g.Children.Add(t);
                t.Text = string.Format("{0}\n{1} scenes\n{2} words\n{3:p1} of story", chap.Model.Name, chap.Scenes.Count(), chap.GetWordCount(), chap.GetWordCount() / totalWordCount);
                t.Margin = new Thickness(1);
                ToolTipService.SetToolTip(t, t.Text);
                Grid.SetRow(t, 1);

                Grid sceneGrid = new Grid();
                g.Children.Add(sceneGrid);

                int sceneCol = 0;

                foreach (var scene in chap.Scenes)
                {
                    cd = new ColumnDefinition();
                    cd.Width = new GridLength(scene.GetWordCount(), GridUnitType.Star);
                    sceneGrid.ColumnDefinitions.Add(cd);

                    b = new Border();
                    sceneGrid.Children.Add(b);
                    Grid.SetColumn(b, sceneCol);
                    b.BorderBrush = new SolidColorBrush(Colors.DarkSlateBlue);
                    b.BorderThickness = new Thickness(1);
                    b.Background = new SolidColorBrush(getNextSceneColor());

                    t = new TextBlock();
                    b.Child = t;
                    t.Text = string.Format("{0}\n{1} words\n{2:p1} of chapter", scene.Model.Name, scene.GetWordCount(), scene.GetWordCount() / (double)chap.GetWordCount());
                    ToolTipService.SetToolTip(b, t.Text);
                    t.Margin = new Thickness(1);
                    t.LayoutTransform = new RotateTransform(270);
                    t.VerticalAlignment = VerticalAlignment.Bottom;
                    t.HorizontalAlignment = HorizontalAlignment.Left;
                    TextOptions.SetTextFormattingMode(t, TextFormattingMode.Display);

                    sceneCol++;
                }

                gridCol++;
            }
        }

        private Color getNextSceneColor()
        {
            colorIndex++;
            if (colorIndex >= colors.Count)
                colorIndex = 0;
            return colors[colorIndex];
        }
    }
}
