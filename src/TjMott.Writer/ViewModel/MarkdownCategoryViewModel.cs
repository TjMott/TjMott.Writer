using System;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class MarkdownCategoryViewModel : MarkdownTreeItem
    {
        #region ICommands
        private ICommand _editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(param => Edit());
                }
                return _editCommand;
            }
        }
        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => Delete());
                }
                return _deleteCommand;
            }
        }
        private ICommand _createDocumentCommand;
        public ICommand CreateDocumentCommand
        {
            get
            {
                if (_createDocumentCommand == null)
                {
                    _createDocumentCommand = new RelayCommand(param => CreateDocument());
                }
                return _createDocumentCommand;
            }
        }
        #endregion

        public MarkdownCategory Model { get; set; }

        public MarkdownCategoryViewModel(MarkdownCategory model, UniverseViewModel universe)
        {
            Model = model;
            UniverseVm = universe;
        }

        public void Edit()
        {
            MarkdownCategoryDialog dialog = new MarkdownCategoryDialog(DialogOwner, UniverseVm.MarkdownTree.Categories, Model);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Save();
                UniverseVm.MarkdownTree.UpdateCategory(this);
            }
            else
            {
                Model.Load();
            }
        }

        public void Delete()
        {
            ConfirmDeleteDialog dialog = new ConfirmDeleteDialog(DialogOwner, "Its subitems will not be removed.");
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Model.Delete();
                if (Parent != null)
                {
                    Parent.Children.Remove(this);
                    foreach (var item in Children)
                    {
                        if (item is MarkdownCategoryViewModel)
                        {
                            MarkdownCategoryViewModel child = (MarkdownCategoryViewModel)item;
                            child.Parent = Parent;
                            child.Model.ParentId = (Parent as MarkdownCategoryViewModel).Model.id;
                            Parent.Children.Add(child);
                            child.Model.Save();
                        }
                        else
                        {
                            MarkdownDocumentViewModel child = (MarkdownDocumentViewModel)item;
                            child.Parent = Parent;

                            MarkdownCategoryDocument link = new MarkdownCategoryDocument(Model.Connection);
                            link.MarkdownCategoryId = (Parent as MarkdownCategoryViewModel).Model.id;
                            link.MarkdownDocumentId = child.Model.id;
                            link.Create();
                            Parent.Children.Add(child);
                        }
                    }
                }
                else
                {
                    if (UniverseVm.MarkdownTree.Items.Contains(this))
                    {
                        UniverseVm.MarkdownTree.Items.Remove(this);
                    }
                    foreach (var item in Children)
                    {
                        if (item is MarkdownCategoryViewModel)
                        {
                            MarkdownCategoryViewModel child = (MarkdownCategoryViewModel)item;
                            child.Parent = null;
                            child.Model.ParentId = null;
                            child.Model.Save();
                            UniverseVm.MarkdownTree.Items.Add(child);
                        }
                        else
                        {
                            MarkdownDocumentViewModel child = (MarkdownDocumentViewModel)item;
                            UniverseVm.MarkdownTree.Items.Add(child);
                        }
                    }
                }
            }
        }

        public void CreateDocument()
        {
            UniverseVm.MarkdownTree.CreateDocument(this);
        }
    }
}
