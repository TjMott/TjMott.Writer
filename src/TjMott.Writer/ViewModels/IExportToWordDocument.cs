using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace TjMott.Writer.ViewModels
{
    public interface IExportToWordDocument
    {
        public class ExportOptions : ViewModelBase
        {
            private string _message = "";
            private string _outputFileName;
            private string _templateFileName;
            private bool _exportEncryptedDocs = false;
            private int _opsProcessed = 0;
            private int _totalOps;

            public string OutputFileName { get => _outputFileName; set { this.RaiseAndSetIfChanged(ref _outputFileName, value); } }
            public string TemplateFileName { get => _templateFileName; set { this.RaiseAndSetIfChanged(ref _templateFileName, value); } }
            public JObject DefaultOpAttributes { get; private set; } = new JObject();
            public bool ExportEncryptedDocs { get => _exportEncryptedDocs; set { this.RaiseAndSetIfChanged(ref _exportEncryptedDocs, value); } }
            public List<string> AesPasswords { get; private set; } = new List<string>();
            public int OpsProcessed { get => _opsProcessed; set { this.RaiseAndSetIfChanged(ref _opsProcessed, value); } }
            public int TotalOps { get => _totalOps; set { this.RaiseAndSetIfChanged(ref _totalOps, value); } }
            public string Message { get => _message; set { this.RaiseAndSetIfChanged(ref _message, value); } }

        }
        Task ExportToWordAsync(DocX doc, ExportOptions exportOptions, CancellationToken cancelToken);
        Task<int> GetOpsCount(Window dialogOwner, ExportOptions exportOptions, CancellationToken cancelToken);
    }
}
