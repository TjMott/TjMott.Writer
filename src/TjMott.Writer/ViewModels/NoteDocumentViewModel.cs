using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels
{
    public class NoteDocumentViewModel : NotesTreeItem
    {
        public NoteDocument Model { get; private set; }

        private Document _document;
        public Document Document { get => _document; private set { _document = value; OnPropertyChanged("Document"); } }

        public NoteDocumentViewModel(NoteDocument model, UniverseViewModel universe)
        {
            Model = model;
            UniverseVm = universe;
        }

        public async void LoadDocument()
        {
            Document doc = new Document(Model.Connection);
            doc.id = Model.DocumentId;
            await doc.LoadAsync();
            Document = doc;
        }

        public void UnloadDocument()
        {
            Document = null;
        }

        public override async Task Rename(Window dialogOwner)
        {
            NameItemWindow renameDialog = new NameItemWindow(Model.Name);
            string newName = await renameDialog.ShowDialog<string>(dialogOwner);
            if (!string.IsNullOrWhiteSpace(newName) && newName != Model.Name)
            {
                Model.Name = newName;
                await Model.SaveAsync();
            }
        }

        public override async Task Delete(Window dialogOwner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Note Category: {0}{1}Its subitems will not be removed.", Model.Name, Environment.NewLine));

            bool result = await dialog.ShowDialog<bool>(dialogOwner);
            if (result)
            {
                await Model.DeleteAsync();
                UniverseVm.NotesTree.RemoveFromTree(this);
            }
        }

        public void OpenInNewWindow()
        {
            NoteWindow.ShowEditorWindow(this);
        }

        public override async Task SetCategories(Window dialogOwner)
        {
            EditNoteDocumentCategoriesWindow dialog = new EditNoteDocumentCategoriesWindow(this, UniverseVm.NotesTree);
            await dialog.ShowDialog(dialogOwner);
        }
    }
}
