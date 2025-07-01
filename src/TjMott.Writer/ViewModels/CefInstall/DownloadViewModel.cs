using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using ReactiveUI;

namespace TjMott.Writer.ViewModels.CefInstall
{
#if FALSE
    public class DownloadViewModel : ViewModelBase
    {
        private string _statusMessage = "";
        public string StatusMessage { get => _statusMessage; private set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }

        private bool _cancelButtonEnabled = false;
        public bool CancelButtonEnabled { get => _cancelButtonEnabled; private set => this.RaiseAndSetIfChanged(ref _cancelButtonEnabled, value); }

        private bool _okButtonEnabled = false;
        public bool OkButtonEnabled { get => _okButtonEnabled; private set => this.RaiseAndSetIfChanged(ref _okButtonEnabled, value); }

        private CancellationTokenSource _cancelToken;
        private bool _installCompleted = false;

        public DownloadViewModel()
        {
            _cancelToken = new CancellationTokenSource();
            BeginDownload();
        }

        public async void BeginDownload()
        {
            CancelButtonEnabled = true;
            try
            {
                StatusMessage = string.Format("Starting download from '{0}'...", CefNetAppImpl.CefDownloadUrl);
                string tempFileBz = Path.Combine(Directory.GetCurrentDirectory(), "cef.tar.bz2");
                string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "tjmott.writer.cefinstall");

                if (File.Exists(tempFileBz))
                    File.Delete(tempFileBz);
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);


                using (HttpClient webClient = new HttpClient())
                {
                    var response = await webClient.GetAsync(CefNetAppImpl.CefDownloadUrl, HttpCompletionOption.ResponseHeadersRead, _cancelToken.Token);
                    if (!response.IsSuccessStatusCode)
                    {
                        StatusMessage = "Download failed.";
                        return;
                    }

                    using (FileStream fs = File.Create(tempFileBz))
                    {
                        long fileLength = response.Content.Headers.ContentLength.Value;
                        long bytesDownloaded = 0;
                        byte[] buffer = new byte[8192];
                        StatusMessage = string.Format("Downloading{2}'{0}'{2}into 'cef.tar.bz2' ({1:P0})...", CefNetAppImpl.CefDownloadUrl, 0.0, Environment.NewLine);
                        using (var contentStream = await response.Content.ReadAsStreamAsync(_cancelToken.Token))
                        {
                            while (bytesDownloaded < fileLength)
                            {
                                int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancelToken.Token);
                                await fs.WriteAsync(buffer, 0, bytesRead, _cancelToken.Token);
                                bytesDownloaded += bytesRead;
                                double progress = (double)bytesDownloaded / fileLength;
                                StatusMessage = string.Format("Downloading{2}'{0}'{2}into 'cef.tar.bz2' ({1:P0})...", CefNetAppImpl.CefDownloadUrl, progress, Environment.NewLine);
                            }
                        }

                        StatusMessage = string.Format("Downloading{2}'{0}'{2}into 'cef.tar.bz2' ({1:P0})...", CefNetAppImpl.CefDownloadUrl, 1.0, Environment.NewLine);
                    }
                }

                CancelButtonEnabled = false; // TAR extract is not cancellable.
                StatusMessage = string.Format("Extracting cef.tar.bz2, {0:P0}...", 0.0);
                using (var fs = File.OpenRead(tempFileBz))
                {
                    using (var bzip = new BZip2InputStream(fs))
                    {
                        var tar = TarArchive.CreateInputTarArchive(bzip, Encoding.UTF8);
                        Task extractTask = Task.Run(() => tar.ExtractContents(tempDir));
                        while (!extractTask.IsCompleted)
                        {
                            StatusMessage = string.Format("Extracting cef.tar.bz2, {0:P0}...", (double)bzip.Position / bzip.Length);
                            await Task.Delay(20);
                        }
                        StatusMessage = string.Format("Extracting cef.tar.bz2, {0:P0}...", 1.0);
                        tar.Close();
                    }
                }
                CancelButtonEnabled = true;

                StatusMessage = string.Format("Installing CEF, {0:P0}...", 0.0);
                string cefPath = CefNetAppImpl.CefAssetPath;
                string libPath = Path.Combine(cefPath, "lib");
                string resourcesPath = Path.Join(cefPath, "resources");
                string localesPath = Path.Join(cefPath, "resources", "locales");

                // Delete local CEF in case this is a re-install.
                if (Directory.Exists(cefPath))
                    Directory.Delete(cefPath, true);
                Directory.CreateDirectory(cefPath);
                Directory.CreateDirectory(libPath);
                Directory.CreateDirectory(resourcesPath);
                Directory.CreateDirectory(localesPath);

                string cefSource = Directory.GetDirectories(tempDir).First();
                string libSource = Path.Combine(cefSource, "Release");
                string resourceSource = Path.Combine(cefSource, "Resources");
                string localesSource = Path.Combine(cefSource, "Resources", "locales");

                int fileCount = 0;
                int filesCopied = 0;
                fileCount += Directory.GetFiles(libSource).Length;
                fileCount += Directory.GetFiles(resourceSource).Length;
                fileCount += Directory.GetFiles(localesSource).Length;

                Task installTask = Task.Run(() =>
                {
                    foreach (var f in Directory.GetFiles(libSource))
                    {
                        _cancelToken.Token.ThrowIfCancellationRequested();
                        string filename = Path.GetFileName(f);
                        File.Copy(f, Path.Combine(libPath, filename));
                        filesCopied++;
                    }
                    foreach (var f in Directory.GetFiles(resourceSource))
                    {
                        _cancelToken.Token.ThrowIfCancellationRequested();
                        string filename = Path.GetFileName(f);
                        File.Copy(f, Path.Combine(resourcesPath, filename));
                        filesCopied++;
                    }
                    foreach (var f in Directory.GetFiles(localesSource))
                    {
                        _cancelToken.Token.ThrowIfCancellationRequested();
                        string filename = Path.GetFileName(f);
                        File.Copy(f, Path.Combine(resourcesPath, "locales", filename));
                        filesCopied++;

                    }
                    File.Copy(Path.Combine(cefSource, "Resources", "icudtl.dat"), Path.Combine(libPath, "icudtl.dat"));
                });

                while (!installTask.IsCompleted)
                {
                    StatusMessage = string.Format("Installing CEF, {0:P0}...", (double)filesCopied / fileCount);
                    await Task.Delay(20);
                }

                StatusMessage = "Cleaning up...";
                await Task.Run(() =>
                {
                    Directory.Delete(tempDir, true);
                    File.Delete(tempFileBz);
                    using (StreamWriter sw = File.CreateText(CefNetAppImpl.CefCookiePath))
                    {
                        sw.Write(CefNetAppImpl.ExpectedCefVersion);
                    }
                });

                StatusMessage = "CEF installed successfully! Re-launch TJ Mott's Writer to finish install.";
                // Delete cookie if necessary.
                if (File.Exists(CefNetAppImpl.CefInstallingCookiePath))
                    File.Delete(CefNetAppImpl.CefInstallingCookiePath);
                _installCompleted = true;
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "CEF installation canceled by user. Press 'Ok' to exit.";
                // Delete cookie if present, since CEF installation is incomplete/corrupted.
                if (File.Exists(CefNetAppImpl.CefCookiePath))
                    File.Delete(CefNetAppImpl.CefCookiePath);
                if (File.Exists(CefNetAppImpl.CefInstallingCookiePath))
                    File.Delete(CefNetAppImpl.CefInstallingCookiePath);
            }
            finally
            {
                OkButtonEnabled = true;
                CancelButtonEnabled = false;
            }
        }

        public void Ok()
        {
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }

        public void Cancel()
        {
            CancelButtonEnabled = false;
            _cancelToken.Cancel();
        }
    }
#endif
}
