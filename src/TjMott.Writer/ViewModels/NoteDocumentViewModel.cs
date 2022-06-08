using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.ViewModels
{
    public class NoteDocumentViewModel : NotesTreeItem
    {
        public NoteDocument Model { get; private set; }

        public NoteDocumentViewModel(NoteDocument model, UniverseViewModel universe)
        {
            Model = model;
            UniverseVm = universe;
        }
    }
}
