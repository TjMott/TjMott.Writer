using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using SixLabors.Fonts;
using Xceed.Words.NET;

namespace TjMott.Writer.ViewModels
{
    public class ExportToWordViewModel : ViewModelBase
    {
        private bool _configViewVisible = true;
        public bool ConfigViewVisible
        {
            get => _configViewVisible;
            private set => this.RaiseAndSetIfChanged(ref _configViewVisible, value);
        }

        private bool _progressViewVisible = false;
        public bool ProgressViewVisible
        {
            get => _progressViewVisible;
            private set => this.RaiseAndSetIfChanged(ref _progressViewVisible, value);
        }

        private string _wordTemplateFile;
        public string WordTemplateFile
        {
            get => _wordTemplateFile;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _wordTemplateFile, value);
                AppSettings.Default.defaultTemplate = value;
                AppSettings.Default.Save();
            }
        }

        private string _outputFile;
        public string OutputFile
        {
            get => _outputFile;
            set => this.RaiseAndSetIfChanged(ref _outputFile, value);
        }

        private bool _exportEncryptedItems = false;
        public bool ExportEncryptedItems
        {
            get => _exportEncryptedItems;
            set => this.RaiseAndSetIfChanged(ref _exportEncryptedItems, value);
        }

        private string _selectedFont;
        public string SelectedFont
        {
            get => _selectedFont;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _selectedFont, value);
                AppSettings.Default.defaultFont = value;
                AppSettings.Default.Save();
            }
        }

        private int _defaultFontSize;
        public int DefaultFontSize
        {
            get =>_defaultFontSize;
            set 
            {
                if (value > 4 && value < 512)
                {
                    this.RaiseAndSetIfChanged(ref _defaultFontSize, value);
                    AppSettings.Default.defaultFontSize = value;
                    AppSettings.Default.Save();
                }
            }
        }

        private string _selectedParagraphAlignment = "Left";
        public string SelectedParagraphAlignment
        {
            get => _selectedParagraphAlignment;
            set => this.RaiseAndSetIfChanged(ref _selectedParagraphAlignment, value);
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            private set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public ObservableCollection<string> FontFamilies { get; private set; }
        public ObservableCollection<string> ParagraphAlignments { get; private set; }

        private IExportToWordDocument _toExport;
        public IExportToWordDocument ItemToExport 
        {
            get => _toExport;
            private set => this.RaiseAndSetIfChanged(ref _toExport, value);
        }

        public ReactiveCommand<Window, Unit> ExportCommand { get; private set; }
        public ReactiveCommand<Window, Unit> BrowseOutputFileCommand { get; private set; }
        public ReactiveCommand<Window, Unit> BrowseTemplateFileCommand { get; private set; }


        private CancellationTokenSource _cancelToken;

        public ExportToWordViewModel(IExportToWordDocument toExport)
        {
            ItemToExport = toExport;

            ExportCommand = ReactiveCommand.CreateFromTask<Window>(exportAsync, this.WhenAnyValue(i => i.OutputFile, i => !string.IsNullOrWhiteSpace(i)));
            BrowseOutputFileCommand = ReactiveCommand.CreateFromTask<Window>(browseOutputFileAsync);
            BrowseTemplateFileCommand = ReactiveCommand.CreateFromTask<Window>(browseTemplateFileAsync);

            loadFonts();
            loadAlignments();

            if (File.Exists(AppSettings.Default.defaultTemplate))
                WordTemplateFile = AppSettings.Default.defaultTemplate;
            DefaultFontSize = AppSettings.Default.defaultFontSize;
        }

        private void loadFonts()
        {
            FontFamilies = new ObservableCollection<string>();
            FontCollection col = new FontCollection();
            col.AddSystemFonts();
            foreach (var font in col.Families.OrderBy(i => i.Name))
            {
                FontFamilies.Add(font.Name);
            }

            if (!string.IsNullOrWhiteSpace(AppSettings.Default.defaultFont) && FontFamilies.Contains(AppSettings.Default.defaultFont))
                SelectedFont = AppSettings.Default.defaultFont;
        }

        private void loadAlignments()
        {
            ParagraphAlignments = new ObservableCollection<string>();
            ParagraphAlignments.Add("Left");
            ParagraphAlignments.Add("Center");
            ParagraphAlignments.Add("Right");
            ParagraphAlignments.Add("Justify");
            SelectedParagraphAlignment = "Left";
        }

        private async Task exportAsync(Window dialogOwner)
        {
            ConfigViewVisible = false;
            ProgressViewVisible = true;
            _cancelToken = new CancellationTokenSource();

            IExportToWordDocument.ExportOptions exportOptions = new IExportToWordDocument.ExportOptions();
            exportOptions.TemplateFileName = WordTemplateFile;
            exportOptions.OutputFileName = OutputFile;
            exportOptions.ExportEncryptedDocs = ExportEncryptedItems;

            // Listener to update progress bar and status messages.
            // Export is so fast that this is kind of useless tbh...
            exportOptions.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(exportOptions.OpsProcessed))
                {
                    Progress = (double)exportOptions.OpsProcessed / exportOptions.TotalOps;
                }
                else if (e.PropertyName == nameof(exportOptions.Message))
                {
                    Status = exportOptions.Message;
                }
            };

            try
            {

                // Get ops count first, so we can calculate progress.
                exportOptions.Message = "Initializing...";
                exportOptions.TotalOps = await ItemToExport.GetOpsCount(dialogOwner, exportOptions, _cancelToken.Token);

                using (DocX doc = DocX.Create(OutputFile))
                {
                    if (File.Exists(WordTemplateFile))
                        doc.ApplyTemplate(WordTemplateFile);

                    JObject defaultAttributes = new JObject();
                    if (!string.IsNullOrWhiteSpace(SelectedFont))
                        exportOptions.DefaultOpAttributes.Add("font", SelectedFont);
                    exportOptions.DefaultOpAttributes.Add("size", DefaultFontSize);
                    if (!string.IsNullOrWhiteSpace(SelectedParagraphAlignment))
                        exportOptions.DefaultOpAttributes.Add("align", SelectedParagraphAlignment.ToLower());

                    await ItemToExport.ExportToWordAsync(doc, exportOptions, _cancelToken.Token);
                    doc.Save();
                }

                var openFile = await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Export Complete",
                                    "Export complete. Open document?",
                                    MsBox.Avalonia.Enums.ButtonEnum.YesNo,
                                    MsBox.Avalonia.Enums.Icon.Success,
                                    WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(dialogOwner);
                if (openFile == MsBox.Avalonia.Enums.ButtonResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(OutputFile) { UseShellExecute = true });
                }
                dialogOwner.Close();
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(OutputFile))
                    File.Delete(OutputFile);
                ConfigViewVisible = true;
                ProgressViewVisible = false;
            }
            catch (Exception ex)
            {
                await MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Error While Exporting",
                    ex.Message,
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error,
                    WindowStartupLocation.CenterOwner).ShowWindowDialogAsync(dialogOwner);
                dialogOwner.Close();
            }

        }

        public void Cancel()
        {
            _cancelToken.Cancel();
        }

        public void Close(Window dialogOwner)
        {
            dialogOwner.Close();
        }

        private async Task browseTemplateFileAsync(Window dialogOwner)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            OpenFileDialog selectTemplateDialog = new OpenFileDialog();
            selectTemplateDialog.Filters.Add(new FileDialogFilter() { Name = "Word Templates (*.dotx)", Extensions = new List<string>() { "dotx" } });
#pragma warning restore CS0618 // Type or member is obsolete
            selectTemplateDialog.Directory = Path.Combine(Directory.GetCurrentDirectory(), "WordTemplates");
            selectTemplateDialog.Title = "Select Template File";
            string[] files = await selectTemplateDialog.ShowAsync(dialogOwner);
            if (files == null || files.Length == 0)
                return;
            if (File.Exists(files[0]))
                WordTemplateFile = files[0];
        }

        private async Task browseOutputFileAsync(Window dialogOwner)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filters.Add(new FileDialogFilter() { Name = "Word Document (*.docx)", Extensions = new List<string>() { "docx" } });
#pragma warning restore CS0618 // Type or member is obsolete
            saveFileDialog.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Title = "Exported File";
            string outputFile = await saveFileDialog.ShowAsync(dialogOwner);
            if (string.IsNullOrEmpty(outputFile))
                return;

            OutputFile = outputFile;
        }
    }
}
