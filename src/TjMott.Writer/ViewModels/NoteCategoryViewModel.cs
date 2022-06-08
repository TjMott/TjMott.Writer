using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;

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
    }
}
