using System;
using System.Threading;
using System.Threading.Tasks;
using Xceed.Words.NET;

namespace TjMott.Writer.ViewModels
{
    public interface IExportToWordDocument
    {
        Task ExportToWordAsync(DocX doc, CancellationToken cancelToken);
    }
}
