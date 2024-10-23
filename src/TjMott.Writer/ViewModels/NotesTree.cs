using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels
{
    public class NotesTree : ViewModelBase
    {
        public UniverseViewModel UniverseVm { get; private set; }

        public ObservableCollection<NoteDocumentViewModel> Notes { get; private set; }
        public ObservableCollection<NotesTreeItem> Tree { get; private set; }
        public ObservableCollection<NoteCategoryViewModel> Categories { get; private set; }


        private string _selectedName = "No Document Selected";
        public string SelectedName { get { return _selectedName; } private set { _selectedName = value; OnPropertyChanged("SelectedName"); } }

        private NotesTreeItem _selectedNote;
        public NotesTreeItem SelectedNote 
        { 
            get { return _selectedNote; } 
            set 
            {
                if (_selectedNote != null && _selectedNote != value && _selectedNote is NoteDocumentViewModel)
                    (_selectedNote as NoteDocumentViewModel).UnloadDocument();
                _selectedNote = value; 
                OnPropertyChanged("SelectedNote");
                if (_selectedNote != null && _selectedNote is NoteDocumentViewModel)
                {
                    (_selectedNote as NoteDocumentViewModel).LoadDocument();
                    SelectedName = (_selectedNote as NoteDocumentViewModel).Model.Name;
                }
                else
                {
                    SelectedName = "No Document Selected";
                }
            } 
        }

        public NotesTree(UniverseViewModel universe)
        {
            UniverseVm = universe;
            Tree = new ObservableCollection<NotesTreeItem>();
            Notes = new ObservableCollection<NoteDocumentViewModel>();
            Categories = new ObservableCollection<NoteCategoryViewModel>();
        }

        public async Task LoadAsync()
        {
            Notes.Clear();
            Tree.Clear();
            Categories.Clear();

            // Load DB models for this universe.
            List<NoteCategory> dbCategories = (await NoteCategory.LoadAll(UniverseVm.Database.Connection)).Where(i => i.UniverseId == UniverseVm.Model.id).ToList();
            List<NoteCategoryDocument> dbCatDocs = (await NoteCategoryDocument.LoadAll(UniverseVm.Database.Connection)).Where(i => dbCategories.Select(k => k.id).Contains(i.NoteCategoryId)).ToList();
            List<NoteDocument> dbDocs = (await NoteDocument.LoadAll(UniverseVm.Database.Connection)).Where(i => i.UniverseId == UniverseVm.Model.id).ToList();

            // Create ViewModels.
            List<NoteCategoryViewModel> categories = dbCategories.Select(i => new NoteCategoryViewModel(i, UniverseVm)).ToList();
            List<NoteDocumentViewModel> docs = dbDocs.Select(i => new NoteDocumentViewModel(i, UniverseVm)).ToList();

            foreach (var doc in docs)
                Notes.Add(doc);


            // Link up categories.
            foreach (var cat in categories)
            {
                Categories.Add(cat);
                if (cat.Model.ParentId == null)
                {
                    Tree.Add(cat);
                }
                else
                {
                    NoteCategoryViewModel parent = categories.Single(i => i.Model.id == cat.Model.ParentId);
                    cat.Parent = parent;
                    parent.Children.Add(cat);
                }

                // Find any documents that belong to this category.
                var catDocs = dbCatDocs.Where(i => i.NoteCategoryId == cat.Model.id).ToList();
                foreach (var item in catDocs)
                {
                    NoteDocumentViewModel doc = docs.Single(i => i.Model.id == item.NoteDocumentId);
                    doc.Parent = cat;
                    cat.Children.Add(doc);
                }
            }

            // Find any documents that are not in a category.
            foreach (var doc in docs.Where(i => i.Parent == null).ToList())
            {
                Tree.Add(doc);
            }
        }

        public void RemoveFromTree(NoteDocumentViewModel doc)
        {
            if (Tree.Contains(doc))
            {
                Tree.Remove(doc);
            }
            foreach (var c in Categories)
            {
                if (c.Children.Contains(doc))
                {
                    c.Children.Remove(doc);
                }
            }
        }

        public async Task UpdateDocumentCategories(NoteDocumentViewModel doc, IEnumerable<NoteCategoryViewModel> newCategories)
        {
            // First, load all document/category links.
            var currentCategories = await NoteCategoryDocument.GetCategoriesForDocument(doc.Model.Connection, doc.Model.id);

            // Delete any that were removed.
            foreach (var cat in currentCategories)
            {
                NoteCategoryViewModel vm = Categories.SingleOrDefault(i => i.Model.id == cat.NoteCategoryId);
                if (vm != null)
                {
                    await cat.DeleteAsync();
                    vm.Children.Remove(doc);
                }
            }

            // Add new ones.
            foreach (var cat in newCategories)
            {
                NoteCategoryDocument link = currentCategories.SingleOrDefault(i => i.NoteCategoryId == cat.Model.id);
                if (link == null)
                {
                    link = new NoteCategoryDocument(doc.Model.Connection);
                    link.NoteCategoryId = cat.Model.id;
                    link.NoteDocumentId = doc.Model.id;
                    await link.CreateAsync();
                }

                if (!cat.Children.Contains(doc))
                {
                    cat.Children.Add(doc);
                }
            }

            // Add/Remove from uncategorized as necessary.
            if (newCategories.Count() == 0)
            {
                if (Tree.Contains(doc))
                {
                    Tree.Add(doc);
                }
            }
            else
            {
                if (Tree.Contains(doc))
                {
                    Tree.Remove(doc);
                }
            }
            
        }

        public async void CreateNewNoteCategory(Window dialogOwner)
        {
            NameItemWindow nameItemWindow = new NameItemWindow("New Note Category");
            string result = await nameItemWindow.ShowDialog<string>(dialogOwner);
            if (!string.IsNullOrEmpty(result))
            {
                NoteCategory noteCategory = new NoteCategory(UniverseVm.Model.Connection);
                noteCategory.UniverseId = UniverseVm.Model.id;
                noteCategory.Name = result;
                await noteCategory.CreateAsync();
                NoteCategoryViewModel vm = new NoteCategoryViewModel(noteCategory, UniverseVm);
                Categories.Add(vm);
                Tree.Add(vm);
            }
        }

        public async void CreateNewNoteDocument(Window dialogOwner)
        {
            NameItemWindow nameItemWindow = new NameItemWindow("New Note Document");
            string result = await nameItemWindow.ShowDialog<string>(dialogOwner);
            if (!string.IsNullOrEmpty(result))
            {
                Document doc = new Document(UniverseVm.Model.Connection);
                doc.UniverseId = UniverseVm.Model.id;
                string json, plainText;
                Document.GetNewDocumentContent(out json, out plainText);
                doc.Json = json;
                doc.PlainText = plainText;
                doc.DocumentType = "Note";
                await doc.CreateAsync();

                NoteDocument noteDoc = new NoteDocument(UniverseVm.Model.Connection);
                noteDoc.UniverseId = UniverseVm.Model.id;
                noteDoc.Name = result;
                noteDoc.DocumentId = doc.id;
                await noteDoc.CreateAsync();

                NoteDocumentViewModel vm = new NoteDocumentViewModel(noteDoc, UniverseVm);
                Tree.Add(vm);
            }
        }
    }
}
