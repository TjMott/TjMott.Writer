using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class PacingWindow : Window
    {
        #region Instance stuff
        private static Dictionary<string, PacingWindow> _windows = new Dictionary<string, PacingWindow>();

        public static void ShowPacingWindow(IEnumerable<ChapterViewModel> chapters, string title, string id)
        {
            if (!_windows.ContainsKey(id))
            {
                _windows[id] = new PacingWindow(chapters, title, id);
            }

            _windows[id].Show();
            _windows[id].Activate();
            _windows[id].Focus();
        }
        #endregion


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
        private string _id;

        public PacingWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private PacingWindow(IEnumerable<ChapterViewModel> chapters, string title, string id)
        {
            _chapters = chapters;
            _id = id;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Title = title;
            Closing += PacingWindow_Closing;
            Activated += PacingWindow_Activated;
            OpenWindowsViewModel.Instance.PacingWindows.Add(this);
        }

        private void PacingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= PacingWindow_Closing;
            if (_windows.ContainsKey(_id))
            {
                _windows.Remove(_id);
            }
            if (OpenWindowsViewModel.Instance.PacingWindows.Contains(this))
            {
                OpenWindowsViewModel.Instance.PacingWindows.Remove(this);
            }
        }

        private void PacingWindow_Activated(object sender, System.EventArgs e)
        {
            Activated -= PacingWindow_Activated;
            doRender();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void doRender()
        {
            Grid grid = this.FindControl<Grid>("chapterGrid");

            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
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
                grid.ColumnDefinitions.Add(cd);

                Border b = new Border();
                grid.Children.Add(b);
                Grid.SetColumn(b, gridCol);
                b.BorderBrush = new SolidColorBrush(Colors.Black);
                b.BorderThickness = new Thickness(2);

                Grid g = new Grid();
                b.Child = g;
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                TextBlock t = new TextBlock();
                g.Children.Add(t);
                t.Text = string.Format("{0}\n{1} scenes\n{2} words\n{3:p1} of story", chap.Model.Name, chap.Scenes.Count, chap.GetWordCount(), chap.GetWordCount() / totalWordCount);
                t.Margin = new Thickness(1);
                ToolTip.SetTip(t, t.Text);
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

                    LayoutTransformControl tContainer = new LayoutTransformControl();
                    tContainer.LayoutTransform = new RotateTransform(270);
                    b.Child = tContainer;
                    tContainer.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                    tContainer.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                    tContainer.Margin = new Thickness(1);

                    t = new TextBlock();
                    tContainer.Child = t;
                    t.Text = string.Format("{0}\n{1} words\n{2:p1} of chapter", scene.Model.Name, scene.GetWordCount(), scene.GetWordCount() / (double)chap.GetWordCount());
                    ToolTip.SetTip(b, t.Text);

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
