using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using TjMott.Writer.Models;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.ViewModels.Search;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels
{
    public class UniverseViewModel : ViewModelBase, ISortable, IGetWordCount
    {
        #region Public properties
        public Universe Model { get; private set; }
        public Database Database { get; private set; }

        public BindingList<CategoryViewModel> Categories { get; private set; }
        public BindingList<StoryViewModel> Stories { get; private set; }
        public SortBySortIndexBindingList<IUniverseSubItem> SubItems { get; private set; }

        private ISortable _selectedStorySubItem;
        public ISortable SelectedStorySubItem
        {
            get => _selectedStorySubItem;
            set => this.RaiseAndSetIfChanged(ref _selectedStorySubItem, value);
        }

        private ISortable _selectedCategoryOrStory;
        public ISortable SelectedCategoryOrStory
        {
            get => _selectedCategoryOrStory; 
            set
            {
                if (value != _selectedCategoryOrStory)
                {
                    if (_selectedCategoryOrStory != null && _selectedCategoryOrStory is StoryViewModel)
                    {
                        var oldStory = _selectedCategoryOrStory as StoryViewModel;
                        oldStory.UnloadChapters();
                    }
                    this.RaiseAndSetIfChanged(ref _selectedCategoryOrStory, value);
                    if (_selectedCategoryOrStory != null && _selectedCategoryOrStory is StoryViewModel)
                    {
                        var newStory = _selectedCategoryOrStory as StoryViewModel;
                        SelectedStoryName = "Story: " + newStory.Model.Name;
                        _ = newStory.LoadChapters();
                    }
                    else
                    {
                        SelectedStoryName = "No story selected";
                    }
                }
            }
        }

        private string _selectedStoryName = "No story selected";
        public string SelectedStoryName
        {
            get => _selectedStoryName;
            private set => this.RaiseAndSetIfChanged(ref _selectedStoryName, value);
        }

        private SearchViewModel _searchViewModel;
        public SearchViewModel SearchViewModel
        {
            get { return _searchViewModel; }
            set
            {
                _searchViewModel = value;
                OnPropertyChanged("SearchViewModel");
            }
        }

        #endregion

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
        public ReactiveCommand<Window, Unit> CreateCategoryCommand { get; }
        public ReactiveCommand<Window, Unit> CreateStoryCommand { get; }
        public ReactiveCommand<object, Unit> MoveItemUpCommand { get; }
        public ReactiveCommand<object, Unit> MoveItemDownCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportToWordCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportStoryToWordCommand { get; }
        public ReactiveCommand<Window, Unit> ShowCategoryOrStoryWordCountCommand { get; }
        public ReactiveCommand<Window, Unit> ShowWordCountCommand { get; }
        public ReactiveCommand<Window, Unit> RenameCommand { get; }
        #endregion

        public async Task<long> GetWordCountAsync()
        {
            long wordCount = 0;
            foreach (var story in Stories)
            {
                wordCount += await story.GetWordCountAsync();
            }
            return wordCount;
        }

        public UniverseViewModel(Universe model, Database database)
        {
            Model = model;
            Database = database;
            model.PropertyChanged += Model_PropertyChanged;
            CreateCategoryCommand = ReactiveCommand.CreateFromTask<Window>(CreateCategory);
            CreateStoryCommand = ReactiveCommand.CreateFromTask<Window>(CreateStory);
            MoveItemUpCommand = ReactiveCommand.Create<object>(MoveItemUp);
            MoveItemDownCommand = ReactiveCommand.Create<object>(MoveItemDown);
            OpenEditorCommand = ReactiveCommand.Create(OpenEditor, this.WhenAny(x => x.SelectedStorySubItem, (item) => (item.Value as SceneViewModel) != null));
            ExportToWordCommand = ReactiveCommand.Create(ExportToWord);
            ExportStoryToWordCommand = ReactiveCommand.CreateFromTask(ExportStoryToWord);
            ShowWordCountCommand = ReactiveCommand.CreateFromTask<Window>(ShowWordCount);
            ShowCategoryOrStoryWordCountCommand = ReactiveCommand.CreateFromTask<Window>(ShowStoryOrCategoryWordCount);
            RenameCommand = ReactiveCommand.CreateFromTask<Window>(Rename);

            Categories = new BindingList<CategoryViewModel>();
            Stories = new BindingList<StoryViewModel>();
            SubItems = new SortBySortIndexBindingList<IUniverseSubItem>();

            SearchViewModel = new SearchViewModel(this);
        }

        public async Task CreateCategory(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow("New Category");
            string result = await dialog.ShowDialog<string>(owner);
            if (!string.IsNullOrWhiteSpace(result))
            {
                Category category = new Category(Model.Connection);
                category.UniverseId = Model.id;
                category.Name = result;
                if (SubItems.Count > 0)
                    category.SortIndex = SubItems.Max(i => i.SortIndex) + 1;
                await category.CreateAsync();

                CategoryViewModel catVm = new CategoryViewModel(category);
                catVm.UniverseVm = this;
                Categories.Add(catVm);
                SubItems.Add(catVm);
            }
       }

        public async Task CreateStory(Window owner)
        {
            Story story = new Story(Model.Connection);
            story.UniverseId = Model.id;

            EditStoryPropertiesWindow dialog = new EditStoryPropertiesWindow();
            dialog.DataContext = new EditStoryPropertiesWindowViewModel(story, Categories);
            bool result = await dialog.ShowDialog<bool>(owner);
            if (result)
            {
                if (story.CategoryId == null)
                {
                    if (SubItems.Count > 0)
                        story.SortIndex = SubItems.Max(i => i.SortIndex) + 1;
                    else
                        story.SortIndex = 0;
                }
                else
                {
                    if (Stories.Count(i => i.Model.CategoryId == story.CategoryId) > 0)
                        story.SortIndex = Stories.Where(i => i.Model.CategoryId == story.CategoryId).Max(i => i.SortIndex + 1);
                    else
                        story.SortIndex = 0;
                }
                await story.CreateAsync();
                StoryViewModel storyVm = new StoryViewModel(story);
                storyVm.UniverseVm = this;
                Stories.Add(storyVm);
                UpdateStoryInTree(storyVm);
            }
        }

        public void UpdateStoryInTree(StoryViewModel story)
        {
            if (story.Model.CategoryId == null)
            {
                var cat = Categories.SingleOrDefault(i => i.Stories.Contains(story));
                if (cat != null)
                {
                    cat.Stories.Remove(story);
                }
                if (!SubItems.Contains(story))
                    SubItems.Add(story);
            }
            else
            {
                var cat = Categories.SingleOrDefault(i => i.Stories.Contains(story));
                if (cat != null && cat.Model.id != story.Model.CategoryId)
                {
                    cat.Stories.Remove(story);
                }
                else
                {
                    cat = Categories.Single(i => i.Model.id == story.Model.CategoryId);
                    if (!cat.Stories.Contains(story))
                        cat.Stories.Add(story);
                }
            }
        }

        public async Task DeleteSubItem(IUniverseSubItem subItem)
        {
            if (subItem is StoryViewModel)
            {
                StoryViewModel story = (StoryViewModel)subItem;
                Stories.Remove(story);
                if (story.Model.CategoryId != null)
                {
                    CategoryViewModel cat = Categories.Single(i => i.Model.id == story.Model.CategoryId);
                    cat.Stories.Remove(story);
                }
            }
            else if (subItem is CategoryViewModel)
            {
                CategoryViewModel cat = (CategoryViewModel)subItem;
                List<StoryViewModel> stories = cat.Stories.ToList();
                foreach (var story in stories)
                {
                    story.Model.CategoryId = null;
                    story.SortIndex = SubItems.Max(i => i.SortIndex) + 1;
                    await story.SaveAsync();
                    SubItems.Add(story);
                }
                Categories.Remove(cat);
                SubItems.Remove(cat);
            }
        }
        public bool CanMoveItemUp(object item)
        {
            if (item == null)
                return false;
            if (item is CategoryViewModel)
            {
                return SubItems.CanMoveItemUp(item as CategoryViewModel);
            }
            else if (item is StoryViewModel)
            {
                StoryViewModel vm = item as StoryViewModel;
                if (vm.Model.CategoryId == null)
                {
                    return SubItems.CanMoveItemUp(vm);
                }
                else
                {
                    CategoryViewModel category = Categories.Single(i => i.Model.id == vm.Model.CategoryId);
                    return category.Stories.CanMoveItemUp(vm);
                }
            }
            else if (item is ChapterViewModel)
            {
                ChapterViewModel vm = item as ChapterViewModel;
                return vm.StoryVm.Chapters.CanMoveItemUp(vm);
            }
            else if (item is SceneViewModel)
            {
                SceneViewModel vm = item as SceneViewModel;
                return vm.ChapterVm.Scenes.CanMoveItemUp(vm);
            }

            return false;
        }
        public bool CanMoveItemDown(object item)
        {
            if (item == null)
                return false;
            if (item is CategoryViewModel)
            {
                return SubItems.CanMoveItemDown(item as CategoryViewModel);
            }
            else if (item is StoryViewModel)
            {
                StoryViewModel vm = item as StoryViewModel;
                if (vm.Model.CategoryId == null)
                {
                    return SubItems.CanMoveItemDown(vm);
                }
                else
                {
                    CategoryViewModel category = Categories.Single(i => i.Model.id == vm.Model.CategoryId);
                    return category.Stories.CanMoveItemDown(vm);
                }
            }
            else if (item is ChapterViewModel)
            {
                ChapterViewModel vm = item as ChapterViewModel;
                return vm.StoryVm.Chapters.CanMoveItemDown(vm);
            }
            else if (item is SceneViewModel)
            {
                SceneViewModel vm = item as SceneViewModel;
                return vm.ChapterVm.Scenes.CanMoveItemDown(vm);
            }
            return false;
        }

        public void MoveItemUp(object item)
        {
            if (item == null)
                return;

            if (item is CategoryViewModel)
            {
                CategoryViewModel vm = item as CategoryViewModel;
                SubItems.MoveItemUp(vm);
            }
            else if (item is StoryViewModel)
            {
                StoryViewModel vm = item as StoryViewModel;
                if (vm.Model.CategoryId == null)
                {
                    SubItems.MoveItemUp(vm);
                }
                else
                {
                    CategoryViewModel category = Categories.Single(i => i.Model.id == vm.Model.CategoryId);
                    category.Stories.MoveItemUp(vm);
                }
            }
            else if (item is ChapterViewModel)
            {
                ChapterViewModel vm = item as ChapterViewModel;
                vm.StoryVm.Chapters.MoveItemUp(vm);
            }
            else if (item is SceneViewModel)
            {
                SceneViewModel vm = item as SceneViewModel;
                vm.ChapterVm.Scenes.MoveItemUp(vm);
            }
        }

        public void MoveItemDown(object item)
        {
            if (item == null)
                return;
            if (item is CategoryViewModel)
            {
                CategoryViewModel vm = item as CategoryViewModel;
                SubItems.MoveItemDown(vm);
            }
            else if (item is StoryViewModel)
            {
                StoryViewModel vm = item as StoryViewModel;
                if (vm.Model.CategoryId == null)
                {
                    SubItems.MoveItemDown(vm);
                }
                else
                {
                    CategoryViewModel category = Categories.Single(i => i.Model.id == vm.Model.CategoryId);
                    category.Stories.MoveItemDown(vm);
                }
            }
            else if (item is ChapterViewModel)
            {
                ChapterViewModel vm = item as ChapterViewModel;
                vm.StoryVm.Chapters.MoveItemDown(vm);
            }
            else if (item is SceneViewModel)
            {
                SceneViewModel vm = item as SceneViewModel;
                vm.ChapterVm.Scenes.MoveItemDown(vm);
            }
        }

        public async Task Rename(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow(Model.Name);
            string result = await dialog.ShowDialog<string>(owner);
            if (result != null)
            {
                Model.Name = result;
                await Model.SaveAsync();
            }
        }

        public void OpenEditor()
        {
            SceneViewModel vm = SelectedStorySubItem as SceneViewModel;
            SceneEditorWindow.ShowEditorWindow(vm);
        }

        public async Task ExportStoryToWord()
        {
            if (SelectedCategoryOrStory != null && SelectedCategoryOrStory is StoryViewModel)
            {
                StoryViewModel vm = SelectedCategoryOrStory as StoryViewModel;
                if (vm.Chapters.Count == 0)
                    await vm.LoadChapters();
                ExportToWordWindow.ShowExportWindow(vm);
            }
        }

        public void ExportToWord()
        {
            if (SelectedStorySubItem != null)
            {
                IExportToWordDocument item = SelectedStorySubItem as IExportToWordDocument;
                ExportToWordWindow.ShowExportWindow(item);
            }
        }

        public async Task ShowWordCount(Window dialogOwner)
        {
            if (SelectedStorySubItem != null)
            {
                IGetWordCount item = SelectedStorySubItem as IGetWordCount;
                var dialog = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Word Count",
                    string.Format("Word Count: {0}", await item.GetWordCountAsync()),
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Info);
                await dialog.ShowWindowDialogAsync(dialogOwner);
            }
        }

        public async Task ShowStoryOrCategoryWordCount(Window dialogOwner)
        {
            if (SelectedCategoryOrStory != null)
            {
                IGetWordCount item = SelectedCategoryOrStory as IGetWordCount;
                var dialog = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Word Count",
                    string.Format("Word Count: {0}", await item.GetWordCountAsync()),
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Info);
                await dialog.ShowWindowDialogAsync(dialogOwner);
            }
        }
    }
}
