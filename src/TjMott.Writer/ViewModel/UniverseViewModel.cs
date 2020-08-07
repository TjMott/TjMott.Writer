using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model;
using TjMott.Writer.Model.SQLiteClasses;
using TjMott.Writer.ViewModel.Search;
using TjMott.Writer.Windows;

namespace TjMott.Writer.ViewModel
{
    public class UniverseViewModel : ViewModelBase, ISortable, IGetWordCount
    {
        #region Public properties
        public Universe Model { get; private set; }
        public Database Database { get; private set; }

        public SortBySortIndexBindingList<CategoryViewModel> Categories { get; private set; }
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

        private object _selectedTreeViewItem;
        public object SelectedTreeViewItem
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

        private SpellcheckDictionary _spellcheckDictionary;
        public SpellcheckDictionary SpellcheckDictionary
        {
            get { return _spellcheckDictionary; }
            private set
            {
                _spellcheckDictionary = value;
                OnPropertyChanged("SpellcheckDictionary");
            }
        }

        private MarkdownTree _markdownTree;
        public MarkdownTree MarkdownTree
        {
            get { return _markdownTree; }
            set
            {
                _markdownTree = value;
                OnPropertyChanged("MarkdownTree");
            }
        }

        private FileBrowserViewModel _fileBrowserVm;
        public FileBrowserViewModel FileBrowserViewModel
        {
            get { return _fileBrowserVm; }
            set
            {
                _fileBrowserVm = value;
                OnPropertyChanged("FileBrowserViewModel");
            }
        }

        private TicketTrackerViewModel _ticketTracker;
        public TicketTrackerViewModel TicketTrackerViewModel
        {
            get { return _ticketTracker; }
            set
            {
                _ticketTracker = value;
                OnPropertyChanged("TicketTrackerViewModel");
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
        public void Save()
        {
            Model.Save();
        }
        #endregion

        #region ICommands
        private ICommand _selectUniverseCommand;
        public ICommand SelectUniverseCommand
        {
            get
            {
                if (_selectUniverseCommand == null)
                {
                    _selectUniverseCommand = new RelayCommand(param => SelectUniverse());
                }
                return _selectUniverseCommand;
            }
        }

        private ICommand _createCategoryCommand;
        public ICommand CreateCategoryCommand
        {
            get
            {
                if (_createCategoryCommand == null)
                {
                    _createCategoryCommand = new RelayCommand(param => CreateCategory());
                }
                return _createCategoryCommand;
            }
        }
        private ICommand _createStoryCommand;
        public ICommand CreateStoryCommand
        {
            get
            {
                if (_createStoryCommand == null)
                {
                    _createStoryCommand = new RelayCommand(param => CreateStory());
                }
                return _createStoryCommand;
            }
        }

        private ICommand _moveItemUpCommand;
        public ICommand MoveItemUpCommand
        {
            get
            {
                if (_moveItemUpCommand == null)
                {
                    _moveItemUpCommand = new RelayCommand(param => MoveItemUp(), param => CanMoveItemUp());
                }
                return _moveItemUpCommand;
            }
        }

        private ICommand _moveItemDownCommand;
        public ICommand MoveItemDownCommand
        {
            get
            {
                if (_moveItemDownCommand == null)
                {
                    _moveItemDownCommand = new RelayCommand(param => MoveItemDown(), param => CanMoveItemDown());
                }
                return _moveItemDownCommand;
            }
        }

        private ICommand _openEditorCommand;
        public ICommand OpenEditorCommand
        {
            get
            {
                if (_openEditorCommand == null)
                {
                    _openEditorCommand = new RelayCommand(param => OpenEditor(), param => CanOpenEditor());
                }
                return _openEditorCommand;
            }
        }

        private ICommand _exportToWordCommand;
        public ICommand ExportToWordCommand
        {
            get
            {
                if (_exportToWordCommand == null)
                {
                    _exportToWordCommand = new RelayCommand(param => ExportToWord(), param => CanExportToWord());
                }
                return _exportToWordCommand;
            }
        }

        private ICommand _showWordCountCommand;
        public ICommand ShowWordCountCommand
        {
            get
            {
                if (_showWordCountCommand == null)
                {
                    _showWordCountCommand = new RelayCommand(param => ShowWordCount(), param => CanShowWordCount());
                }
                return _showWordCountCommand;
            }
        }
        private ICommand _renameCommand;
        public ICommand RenameCommand
        {
            get
            {
                if (_renameCommand == null)
                {
                    _renameCommand = new RelayCommand(param => Rename());
                }
                return _renameCommand;
            }
        }
        #endregion

        public UniverseViewModel(Universe model, Database database)
        {
            Model = model;
            Database = database;

            model.PropertyChanged += Model_PropertyChanged;

            Categories = new SortBySortIndexBindingList<CategoryViewModel>();
            Stories = new BindingList<StoryViewModel>();
            SubItems = new SortBySortIndexBindingList<IUniverseSubItem>();

            SearchViewModel = new SearchViewModel();
            SearchViewModel.SelectedUniverse = this;
            SpellcheckDictionary = new SpellcheckDictionary(this);

            MarkdownTree = new MarkdownTree(this);
            MarkdownTree.Load();
            FileBrowserViewModel = new FileBrowserViewModel(this);
            FileBrowserViewModel.Load();

            TicketTrackerViewModel = new TicketTrackerViewModel(this);
            TicketTrackerViewModel.Load();
        }

        public void UpdateSubItemSortIndices()
        {
            for (int i = 0; i < SubItems.Count; i++)
            {
                SubItems[i].SortIndex = i;
                SubItems[i].Save();
            }
        }


        public void SelectUniverse()
        {
            Database.SelectedUniverse = this;
        }

        public void CreateCategory()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, "New Category");
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Category category = new Category(Model.Connection);
                category.UniverseId = Model.id;
                category.Name = dialog.UserInput;
                if (SubItems.Count > 0)
                    category.SortIndex = SubItems.Max(i => i.SortIndex) + 1;
                category.Create();

                CategoryViewModel catVm = new CategoryViewModel(category);
                catVm.UniverseVm = this;
                Categories.Add(catVm);
                SubItems.Add(catVm);
            }
        }

        public void CreateStory()
        {
            Story story = new Story(Model.Connection);
            story.UniverseId = Model.id;

            StoryPropertiesDialog dialog = new StoryPropertiesDialog(DialogOwner, story, Categories);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
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
                story.Create();
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

        public void DeleteSubItem(IUniverseSubItem subItem)
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
                foreach (var story in cat.Stories)
                {
                    story.Model.CategoryId = null;
                    story.SortIndex = SubItems.Max(i => i.SortIndex) + 1;
                    story.Save();
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

        public void Rename()
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, Model.Name);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Name = dialog.UserInput;
                Model.Save();
            }
        }

        public bool CanOpenEditor()
        {
            SceneViewModel vm = SelectedTreeViewItem as SceneViewModel;
            return vm != null;
        }

        public void OpenEditor()
        {
            SceneViewModel vm = SelectedTreeViewItem as SceneViewModel;
            FlowDocumentEditorWindow.ShowEditorWindow(vm.Model.FlowDocumentId, vm.Model.Connection, SpellcheckDictionary, string.Format("Scene: {0}", vm.Model.Name));
        }

        public bool CanExportToWord()
        {
            return (SelectedTreeViewItem as IExportToWordDocument) != null;
        }

        public void ExportToWord()
        {
            IExportToWordDocument item = SelectedTreeViewItem as IExportToWordDocument;

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
            }
        }

        public bool CanShowWordCount()
        {
            return (SelectedTreeViewItem as IGetWordCount) != null;
        }
        public void ShowWordCount()
        {
            IGetWordCount item = SelectedTreeViewItem as IGetWordCount;
        }
    }
}
