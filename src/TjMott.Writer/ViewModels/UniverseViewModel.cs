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

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        private ISortable _selectedTreeViewItem;
        public ISortable SelectedTreeViewItem
        {
            get { return _selectedTreeViewItem; }
            set
            {
                _selectedTreeViewItem = value;
                OnPropertyChanged("SelectedTreeViewItem");
            }
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

        private NotesTree _notesTree;
        public NotesTree NotesTree
        {
            get { return _notesTree; }
            set
            {
                _notesTree = value;
                OnPropertyChanged("NotesTree");
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
        public ReactiveCommand<Unit, Unit> SelectUniverseCommand { get; }
        public ReactiveCommand<Window, Unit> CreateCategoryCommand { get; }
        public ReactiveCommand<Window, Unit> CreateStoryCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveItemUpCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveItemDownCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenEditorCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportToWordCommand { get; }
        public ReactiveCommand<Window, Unit> ShowWordCountCommand { get; }
        public ReactiveCommand<Window, Unit> RenameCommand { get; }
        #endregion

        public long GetWordCount()
        {
            return Stories.Sum(i => i.GetWordCount());
        }

        public UniverseViewModel(Universe model, Database database)
        {
            Model = model;
            Database = database;
            model.PropertyChanged += Model_PropertyChanged;
            SelectUniverseCommand = ReactiveCommand.Create(SelectUniverse);
            CreateCategoryCommand = ReactiveCommand.Create<Window>(CreateCategory);
            CreateStoryCommand = ReactiveCommand.Create<Window>(CreateStory);
            MoveItemUpCommand = ReactiveCommand.Create(MoveItemUp, this.WhenAny(x => x.SelectedTreeViewItem.SortIndex, (item) => CanMoveItemUp()));
            MoveItemDownCommand = ReactiveCommand.Create(MoveItemDown, this.WhenAny(x => x.SelectedTreeViewItem.SortIndex, (item) => CanMoveItemDown()));
            OpenEditorCommand = ReactiveCommand.Create(OpenEditor, this.WhenAny(x => x.SelectedTreeViewItem, (item) => (item.Value as SceneViewModel) != null));
            ExportToWordCommand = ReactiveCommand.Create(ExportToWord, this.WhenAny(x => x.SelectedTreeViewItem, (item) => (item.Value as IExportToWordDocument) != null));
            ShowWordCountCommand = ReactiveCommand.Create<Window>(ShowWordCount, this.WhenAny(x => x.SelectedTreeViewItem, (item) => (item.Value as IGetWordCount) != null));
            RenameCommand = ReactiveCommand.Create<Window>(Rename);

            initializeAsync();
        }

        private async void initializeAsync()
        {
            Categories = new BindingList<CategoryViewModel>();
            Stories = new BindingList<StoryViewModel>();
            SubItems = new SortBySortIndexBindingList<IUniverseSubItem>();

            NotesTree = new NotesTree(this);
            await NotesTree.LoadAsync().ConfigureAwait(false);

            SearchViewModel = new SearchViewModel();
            SearchViewModel.SelectedUniverse = this;
        }

        public void UpdateSubItemSortIndices()
        {
            for (int i = 0; i < SubItems.Count; i++)
            {
                SubItems[i].SortIndex = i;
                SubItems[i].SaveAsync();
            }
        }


        public void SelectUniverse()
        {
            Database.SelectedUniverse = this;
        }

        public async void CreateCategory(Window owner)
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
                await category.CreateAsync().ConfigureAwait(false);

                CategoryViewModel catVm = new CategoryViewModel(category);
                catVm.UniverseVm = this;
                Categories.Add(catVm);
                SubItems.Add(catVm);
            }
       }

        public async void CreateStory(Window owner)
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
                await story.CreateAsync().ConfigureAwait(false);
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
                    cat.UpdateStorySortIndices();
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
                    cat.UpdateStorySortIndices();
                }
                else
                {
                    cat = Categories.Single(i => i.Model.id == story.Model.CategoryId);
                    if (!cat.Stories.Contains(story))
                        cat.Stories.Add(story);
                }
            }
        }

        public async void DeleteSubItem(IUniverseSubItem subItem)
        {
            if (subItem is StoryViewModel)
            {
                StoryViewModel story = (StoryViewModel)subItem;
                Stories.Remove(story);
                if (story.Model.CategoryId != null)
                {
                    CategoryViewModel cat = Categories.Single(i => i.Model.id == story.Model.CategoryId);
                    cat.Stories.Remove(story);
                    cat.UpdateStorySortIndices();
                }
                UpdateSubItemSortIndices();
            }
            else if (subItem is CategoryViewModel)
            {
                CategoryViewModel cat = (CategoryViewModel)subItem;
                List<StoryViewModel> stories = cat.Stories.ToList();
                foreach (var story in stories)
                {
                    story.Model.CategoryId = null;
                    story.SortIndex = SubItems.Max(i => i.SortIndex) + 1;
                    await story.SaveAsync().ConfigureAwait(false);
                    SubItems.Add(story);
                }
                Categories.Remove(cat);
                SubItems.Remove(cat);
                UpdateSubItemSortIndices();
            }
        }
        public bool CanMoveItemUp()
        {
            if (SelectedTreeViewItem == null)
                return false;
            if (SelectedTreeViewItem is CategoryViewModel)
            {
                return SubItems.CanMoveItemUp(SelectedTreeViewItem as CategoryViewModel);
            }
            else if (SelectedTreeViewItem is StoryViewModel)
            {
                StoryViewModel vm = SelectedTreeViewItem as StoryViewModel;
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
            else if (SelectedTreeViewItem is ChapterViewModel)
            {
                ChapterViewModel vm = SelectedTreeViewItem as ChapterViewModel;
                return vm.StoryVm.Chapters.CanMoveItemUp(vm);
            }
            else if (SelectedTreeViewItem is SceneViewModel)
            {
                SceneViewModel vm = SelectedTreeViewItem as SceneViewModel;
                return vm.ChapterVm.Scenes.CanMoveItemUp(vm);
            }

            return false;
        }
        public bool CanMoveItemDown()
        {
            if (SelectedTreeViewItem == null)
                return false;
            if (SelectedTreeViewItem is CategoryViewModel)
            {
                return SubItems.CanMoveItemDown(SelectedTreeViewItem as CategoryViewModel);
            }
            else if (SelectedTreeViewItem is StoryViewModel)
            {
                StoryViewModel vm = SelectedTreeViewItem as StoryViewModel;
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
            else if (SelectedTreeViewItem is ChapterViewModel)
            {
                ChapterViewModel vm = SelectedTreeViewItem as ChapterViewModel;
                return vm.StoryVm.Chapters.CanMoveItemDown(vm);
            }
            else if (SelectedTreeViewItem is SceneViewModel)
            {
                SceneViewModel vm = SelectedTreeViewItem as SceneViewModel;
                return vm.ChapterVm.Scenes.CanMoveItemDown(vm);
            }
            return false;
        }

        public void MoveItemUp()
        {
            if (SelectedTreeViewItem is CategoryViewModel)
            {
                CategoryViewModel vm = SelectedTreeViewItem as CategoryViewModel;
                SubItems.MoveItemUp(vm);
            }
            else if (SelectedTreeViewItem is StoryViewModel)
            {
                StoryViewModel vm = SelectedTreeViewItem as StoryViewModel;
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
            else if (SelectedTreeViewItem is ChapterViewModel)
            {
                ChapterViewModel vm = SelectedTreeViewItem as ChapterViewModel;
                vm.StoryVm.Chapters.MoveItemUp(vm);
            }
            else if (SelectedTreeViewItem is SceneViewModel)
            {
                SceneViewModel vm = SelectedTreeViewItem as SceneViewModel;
                vm.ChapterVm.Scenes.MoveItemUp(vm);
            }
        }

        public void MoveItemDown()
        {
            if (SelectedTreeViewItem is CategoryViewModel)
            {
                CategoryViewModel vm = SelectedTreeViewItem as CategoryViewModel;
                SubItems.MoveItemDown(vm);
            }
            else if (SelectedTreeViewItem is StoryViewModel)
            {
                StoryViewModel vm = SelectedTreeViewItem as StoryViewModel;
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
            else if (SelectedTreeViewItem is ChapterViewModel)
            {
                ChapterViewModel vm = SelectedTreeViewItem as ChapterViewModel;
                vm.StoryVm.Chapters.MoveItemDown(vm);
            }
            else if (SelectedTreeViewItem is SceneViewModel)
            {
                SceneViewModel vm = SelectedTreeViewItem as SceneViewModel;
                vm.ChapterVm.Scenes.MoveItemDown(vm);
            }
        }

        public async void Rename(Window owner)
        {
            NameItemWindow dialog = new NameItemWindow(Model.Name);
            string result = await dialog.ShowDialog<string>(owner);
            if (result != null)
            {
                Model.Name = result;
                await Model.SaveAsync().ConfigureAwait(false);
            }
        }

        public void OpenEditor()
        {
            SceneViewModel vm = SelectedTreeViewItem as SceneViewModel;
            SceneEditorWindow.ShowEditorWindow(vm);
            //FlowDocumentEditorWindow.ShowEditorWindow(vm.Model.FlowDocumentId, vm.Model.Connection, SpellcheckDictionary, string.Format("Scene: {0}", vm.Model.Name));
        }

        public void ExportToWord()
        {
            /*IExportToWordDocument item = SelectedTreeViewItem as IExportToWordDocument;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Word Document (*.docx)|*.docx";
            bool? result = saveFileDialog.ShowDialog();
            if (!result.Value)
                return;

            var templates = FileBrowserViewModel.Files.Where(i => i.Model.FileType == FileBlob.FILE_TYPE_TEMPLATE).ToList();
            if (templates.Count > 0)
            {
                FileBlobViewModel defaultTemplate = templates.SingleOrDefault(i => i.Model.id == Model.DefaultTemplateId);

                SelectDocTemplateDialog dialog = new SelectDocTemplateDialog(DialogOwner, templates, defaultTemplate);
                result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    defaultTemplate = dialog.SelectedTemplate;
                    string templatePath = null;

                    // Save selected template as default.
                    if (defaultTemplate != null)
                    {
                        Model.DefaultTemplateId = defaultTemplate.Model.id;
                        Model.Save();

                        // Save template to a temp file.
                        templatePath = Path.GetTempFileName();
                        defaultTemplate.Model.ExportToFile(templatePath);
                    }

                    FlowDocumentExporter.ExportItem(item, saveFileDialog.FileName, templatePath);

                    if (templatePath != null)
                    {
                        File.Delete(templatePath);
                    }

                    MessageBoxResult msgResult = MessageBox.Show("Export complete. Open file now?", "Export Completed", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (msgResult == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            else
            {
                FlowDocumentExporter.ExportItem(item, saveFileDialog.FileName, null);
                MessageBoxResult msgResult = MessageBox.Show("Export complete. Open file now?", "Export Completed", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgResult == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
            }*/
        }

        public void ShowWordCount(Window owner)
        {
            IGetWordCount item = SelectedTreeViewItem as IGetWordCount;
            var dialog = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Word Count", string.Format("Word Count: {0}", item.GetWordCount()), MessageBox.Avalonia.Enums.ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info);
            dialog.ShowDialog(owner);
        }
    }
}
