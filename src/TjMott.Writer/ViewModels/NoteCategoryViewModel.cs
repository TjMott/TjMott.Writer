using Avalonia.Controls;
using System;
using TjMott.Writer.Models.SQLiteClasses;
using TjMott.Writer.Views;

namespace TjMott.Writer.ViewModels
{
    public class NoteCategoryViewModel : NotesTreeItem
    {
        public NoteCategory Model { get; private set; }


        public NoteCategoryViewModel(NoteCategory model, UniverseViewModel universe)
        {
            Model = model;
            UniverseVm = universe;
        }

        public override async void Rename(Window dialogOwner)
        {
            NameItemWindow renameDialog = new NameItemWindow(Model.Name);
            string newName = await renameDialog.ShowDialog<string>(dialogOwner);
            if (!string.IsNullOrWhiteSpace(newName) && newName != Model.Name)
            {
                Model.Name = newName;
                await Model.SaveAsync();
            }
        }

        public override async void Delete(Window dialogOwner)
        {
            ConfirmDeleteWindow dialog = new ConfirmDeleteWindow(string.Format("Note Category: {0}{1}Its subitems will not be removed.", Model.Name, Environment.NewLine));

            bool result = await dialog.ShowDialog<bool>(dialogOwner);
            if (result)
            {
                await Model.DeleteAsync();
                if (Parent != null)
                {
                    // Re-assign this node's children to this node's parent.
                    Parent.Children.Remove(this);
                    foreach (var item in Children)
                    {
                        if (item is NoteCategoryViewModel)
                        {
                            NoteCategoryViewModel child = (NoteCategoryViewModel)item;
                            child.Parent = Parent;
                            child.Model.ParentId = (Parent as NoteCategoryViewModel).Model.id;
                            Parent.Children.Add(child);
                            await child.Model.SaveAsync();
                        }
                        else
                        {
                            NoteDocumentViewModel child = (NoteDocumentViewModel)item;
                            child.Parent = Parent;

                            NoteCategoryDocument link = new NoteCategoryDocument(Model.Connection);
                            link.NoteCategoryId = (Parent as NoteCategoryViewModel).Model.id;
                            link.NoteDocumentId = child.Model.id;
                            await link.CreateAsync();
                            Parent.Children.Add(child);
                        }
                    }
                }
                else
                {
                    // No parent. Re-assign this node's children to the root.
                    if (UniverseVm.NotesTree.Items.Contains(this))
                    {
                        UniverseVm.NotesTree.Items.Remove(this);
                        foreach (var item in Children)
                        {
                            if (item is NoteCategoryViewModel)
                            {
                                NoteCategoryViewModel child = (NoteCategoryViewModel)item;
                                child.Parent = null;
                                child.Model.ParentId = null;
                                await child.Model.SaveAsync();
                                UniverseVm.NotesTree.Items.Add(child);
                            }
                            else
                            {
                                NoteDocumentViewModel child = (NoteDocumentViewModel)item;
                                UniverseVm.NotesTree.Items.Add(child);
                            }
                        }
                    }
                }
            }
        }

        public override async void SetCategories(Window dialogOwner)
        {

        }
    }
}
