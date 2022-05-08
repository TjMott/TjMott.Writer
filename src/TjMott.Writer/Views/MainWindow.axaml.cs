using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using TjMott.Writer.Models;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Views
{
    public partial class MainWindow : Window
    {
        public class TestClass : ReactiveObject, ISortable
        {
            private long _sortIndex;
            public long SortIndex { get => _sortIndex; set =>this.RaiseAndSetIfChanged(ref _sortIndex, value); }
            public string Name { get; set; }

            public void Save()
            {
                
            }
        }

        public SortBySortIndexBindingList<TestClass> TestList { get; private set; }
        public TestClass SelectedItem;
        public MainWindow()
        {
            InitializeComponent();
            ViewModelBase.MainWindow = this;

            TestList = new SortBySortIndexBindingList<TestClass>();

            for (int i = 0; i < 5; i++)
            {
                TestClass t = new TestClass() { SortIndex = i, Name = "Item " + i.ToString() };
                TestList.Add(t);
            }

            this.FindControl<ListBox>("testListBox").DataContext = this;
            SelectedItem = TestList[0];
            this.FindControl<ListBox>("testListBox").SelectedItem = SelectedItem;
            UpdateTestText();
        }

        public void upButton_Click(object sender, RoutedEventArgs args)
        {
            if (TestList.CanMoveItemUp(SelectedItem))
                TestList.MoveItemUp(SelectedItem);
            UpdateTestText();
        }

        public void downButton_Click(object sender, RoutedEventArgs args)
        {
            if (TestList.CanMoveItemDown(SelectedItem))
                TestList.MoveItemDown(SelectedItem);
            UpdateTestText();
        }

        public void testListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            SelectedItem = (TestClass)this.FindControl<ListBox>("testListBox").SelectedItem;
        }

        public void UpdateTestText()
        {
            string txt = "";
            foreach (var item in TestList)
                txt += item.Name + "\r\n";
            this.FindControl<TextBlock>("testTextBlock").Text = txt;
        }
    }
}
