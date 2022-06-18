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
    }
}
