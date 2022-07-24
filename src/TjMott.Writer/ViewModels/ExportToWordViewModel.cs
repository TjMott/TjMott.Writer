using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using Xceed.Words.NET;

namespace TjMott.Writer.ViewModels
{
    public class ExportToWordViewModel : ViewModelBase
    {
        private IExportToWordDocument _toExport;
        public IExportToWordDocument ItemToExport { get => _toExport; private set => this.RaiseAndSetIfChanged(ref _toExport, value); }

        private CancellationTokenSource _cancelToken;

        public ExportToWordViewModel(IExportToWordDocument toExport)
        {
            ItemToExport = toExport;
            _cancelToken = new CancellationTokenSource();
        }

        public async Task ExportAsync(Window dialogOwner)
        {
            OpenFileDialog selectTemplateDialog = new OpenFileDialog();
            selectTemplateDialog.Filters.Add(new FileDialogFilter() { Name = "Word Templates (*.dotx)", Extensions = new List<string>() { "dotx" } });
            selectTemplateDialog.Directory = Path.Combine(Directory.GetCurrentDirectory(), "WordTemplates");
            selectTemplateDialog.Title = "Select Template File";
            string[] files = await selectTemplateDialog.ShowAsync(dialogOwner);
            if (files == null || files.Length == 0)
                return;

            string templatePath = files[0];

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filters.Add(new FileDialogFilter() { Name = "Word Document (*.docx)", Extensions = new List<string>() { "docx" } });
            saveFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Title = "Exported File";
            string outputFile = await saveFileDialog.ShowAsync(dialogOwner);
            if (string.IsNullOrEmpty(outputFile))
                return;

            using (DocX doc = DocX.Create(outputFile))
            {
                try
                {
                    doc.ApplyTemplate(templatePath);

                    if (ItemToExport is StoryViewModel)
                    {
                        createTitlePage((StoryViewModel)ItemToExport, doc);
                        createCopyrightPage((StoryViewModel)ItemToExport, doc);
                        createTableOfContents((StoryViewModel)ItemToExport, doc);
                    }

                    await ItemToExport.ExportToWordAsync(doc, _cancelToken.Token);
                }
                catch (OperationCanceledException)
                {
                    if (File.Exists(outputFile))
                        File.Delete(outputFile);
                }
            }

        }

        public void Cancel()
        {
            _cancelToken.Cancel();
        }

        private void createTitlePage(StoryViewModel svm, DocX doc)
        {

        }

        private void createCopyrightPage(StoryViewModel svm, DocX doc)
        {

        }

        private void createTableOfContents(StoryViewModel svm, DocX doc)
        {

        }
    }
}
