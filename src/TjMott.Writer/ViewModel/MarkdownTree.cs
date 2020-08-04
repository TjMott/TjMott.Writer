using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class MarkdownTree : ViewModelBase
    {
        #region ICommands
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
        public UniverseViewModel UniverseVm { get; private set; }

        public BindingList<MarkdownTreeItem> Items { get; private set; }
        public BindingList<MarkdownCategoryViewModel> Categories { get; private set; }

        public MarkdownTree(UniverseViewModel universe)
        {
            UniverseVm = universe;
            Items = new BindingList<MarkdownTreeItem>();
            Categories = new BindingList<MarkdownCategoryViewModel>();
        }

        public void Load()
        {
            Items.Clear();

            List<MarkdownCategory> dbCategories = MarkdownCategory.GetAllMarkdownDocuments(UniverseVm.Model.Connection).Where(i => i.UniverseId == UniverseVm.Model.id).ToList();
            List<MarkdownCategoryDocument> dbCatDocs = MarkdownCategoryDocument.GetAllMarkdownDocuments(UniverseVm.Model.Connection).ToList();
            List<MarkdownDocument> dbDocs = MarkdownDocument.GetAllMarkdownDocuments(UniverseVm.Model.Connection).Where(i => i.UniverseId == UniverseVm.Model.id).ToList();

            List<MarkdownCategoryViewModel> categories = dbCategories.Select(i => new MarkdownCategoryViewModel(i, UniverseVm)).ToList();
            List<MarkdownDocumentViewModel> docs = dbDocs.Select(i => new MarkdownDocumentViewModel(i, UniverseVm)).ToList();

            // Link up categories.
            foreach (var cat in categories)
            {
                Categories.Add(cat);
                cat.UniverseVm = UniverseVm;
                if (cat.Model.ParentId == null)
                {
                    Items.Add(cat);
                }
                else
                {
                    MarkdownCategoryViewModel parent = categories.Single(i => i.Model.id == cat.Model.ParentId);
                    cat.Parent = parent;
                    parent.Children.Add(cat);
                }

                // Find any documents that belong to this category.
                var catDocs = dbCatDocs.Where(i => i.MarkdownCategoryId == cat.Model.id).ToList();
                foreach (var item in catDocs)
                {
                    MarkdownDocumentViewModel doc = docs.Single(i => i.Model.id == item.MarkdownDocumentId);
                    doc.Parent = cat;
                    cat.Children.Add(doc);
                }
            }

            // Find any documents that are not in a category.
            foreach (var doc in docs.Where(i => i.Parent == null).ToList())
            {
                Items.Add(doc);
            }
        }

        public void CreateCategory()
        {
            MarkdownCategory category = new MarkdownCategory(UniverseVm.Model.Connection);
            category.Name = "New Category";
            category.UniverseId = UniverseVm.Model.id;
            MarkdownCategoryDialog dialog = new MarkdownCategoryDialog(DialogOwner, Categories, category);
            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                category.Create();
                MarkdownCategoryViewModel vm = new MarkdownCategoryViewModel(category, UniverseVm);
                Categories.Add(vm);
                if (category.ParentId != null)
                {
                    vm.Parent = Categories.Single(i => i.Model.id == category.ParentId);
                    vm.Parent.Children.Add(vm);
                }
                else
                {
                    Items.Add(vm);
                }
            }
        }

        public void CreateDocument(MarkdownCategoryViewModel parent = null)
        {
            NameItemDialog dialog = new NameItemDialog(DialogOwner, "New Note");
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                MarkdownDocument doc = new MarkdownDocument(UniverseVm.Model.Connection);
                doc.Name = dialog.UserInput;
                doc.UniverseId = UniverseVm.Model.id;
                doc.MarkdownText = string.Format("# {0}\r\nThis is a new note document.", doc.Name);
                doc.PlainText = Markdig.Markdown.ToPlainText(doc.MarkdownText);
                doc.Create();

                MarkdownDocumentViewModel docVm = new MarkdownDocumentViewModel(doc, UniverseVm);

                if (parent != null)
                {
                    MarkdownCategoryDocument link = new MarkdownCategoryDocument(UniverseVm.Model.Connection);
                    link.MarkdownDocumentId = doc.id;
                    link.MarkdownCategoryId = parent.Model.id;
                    link.Create();

                    docVm.Parent = parent;
                    parent.Children.Add(docVm);
                }
                else
                {
                    Items.Add(docVm);
                }
            }
        }

        public void UpdateCategory(MarkdownCategoryViewModel vm)
        {
            if (vm.Model.ParentId == null)
            {
                vm.Parent = null;
                if (!Items.Contains(vm))
                    Items.Add(vm);
            }
            else
            {
                MarkdownCategoryViewModel newParent = Categories.Single(i => i.Model.id == vm.Model.ParentId);
                if (!newParent.Children.Contains(vm))
                {
                    if (vm.Parent != null)
                    {
                        vm.Parent.Children.Remove(vm);
                    }
                    newParent.Children.Add(vm);
                    vm.Parent = newParent;
                }
                if (Items.Contains(vm))
                    Items.Remove(vm);
            }
        }

        public void UpdateDocumentCategories(MarkdownDocumentViewModel doc, IEnumerable<MarkdownCategoryViewModel> newCategories)
        {
            // First, load all document/category links from the database.
            var currentCategories = MarkdownCategoryDocument.GetCategoriesForDocument(doc.Model.Connection, doc.Model.id);

            // Delete any that were removed.
            foreach (var cat in currentCategories)
            {
                MarkdownCategoryViewModel vm = Categories.SingleOrDefault(i => i.Model.id == cat.MarkdownCategoryId);
                if (vm != null)
                {
                    cat.Delete();
                    vm.Children.Remove(doc);
                }
            }

            // Add new ones.
            foreach (var cat in newCategories)
            {
                MarkdownCategoryDocument dbLink = currentCategories.SingleOrDefault(i => i.MarkdownCategoryId == cat.Model.id);
                if (dbLink == null)
                {
                    dbLink = new MarkdownCategoryDocument(doc.Model.Connection);
                    dbLink.MarkdownCategoryId = cat.Model.id;
                    dbLink.MarkdownDocumentId = doc.Model.id;
                    dbLink.Create();
                }

                if (!cat.Children.Contains(doc))
                {
                    cat.Children.Add(doc);
                }
            }

            // Add/Remove from uncategoried as necessary.
            if (newCategories.Count() == 0)
            {
                if (!Items.Contains(doc))
                {
                    Items.Add(doc);
                }
            }
            else
            {
                if (Items.Contains(doc))
                {
                    Items.Remove(doc);
                }
            }
        }

        public void RemoveFromTree(MarkdownDocumentViewModel doc)
        {
            if (Items.Contains(doc))
            {
                Items.Remove(doc);
            }
            foreach (var c in Categories)
            {
                if (c.Children.Contains(doc))
                {
                    c.Children.Remove(doc);
                }
            }
        }
    }
}
