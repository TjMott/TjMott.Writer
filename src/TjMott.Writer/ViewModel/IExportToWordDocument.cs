using System;
using Docx = Xceed.Words.NET;

namespace TjMott.Writer.ViewModel
{
    public interface IExportToWordDocument
    {
        void ExportToWord(Docx.DocX doc);
    }
}
