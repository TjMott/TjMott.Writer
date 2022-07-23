using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CefNet;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Tar;
using ReactiveUI;

namespace TjMott.Writer.ViewModels.CefInstall
{
    public class DownloadViewModel : ViewModelBase
    {
        private string _log = "";
        public string Log { get => _log; private set => this.RaiseAndSetIfChanged(ref _log, value); }

        public DownloadViewModel()
        {
            BeginDownload();
        }

        public async void BeginDownload()
        {
            writeLog(string.Format("Starting download from '{0}'...", CefNetAppImpl.CefDownloadUrl));
            //string tempFileBz = Path.Combine(Directory.GetCurrentDirectory(), "cef.tar.bz2");
            string tempFileBz = @"c:\users\TJ Mott\Desktop\temp.tar.bz";
            //string tempDir = Path.Combine(Directory.GetCurrentDirectory(), "tjmott.writer.cefinstall");
            string tempDir = @"c:\users\TJ Mott\Desktop\cefinstalltemp";
            using (HttpClient webClient = new HttpClient())
            {
                var response = await webClient.GetAsync(CefNetAppImpl.CefDownloadUrl);
                if (!response.IsSuccessStatusCode)
                {
                    writeLog("Download failed.");
                    return;
                }
                
                using (var fs = File.Create(tempFileBz))
                {
                    writeLog("Download started.");
                    await response.Content.CopyToAsync(fs);
                    writeLog("Download completed.");
                }
            }

            writeLog("Extracting file...");
            using (var fs = File.OpenRead(tempFileBz))
            {
                using (var bzip = new BZip2InputStream(fs))
                {
                    var tar = TarArchive.CreateInputTarArchive(bzip, Encoding.UTF8);
                    tar.ExtractContents(tempDir);
                }
            }

            writeLog("Installing CEF...");
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
            foreach (var f in Directory.GetFiles(Path.Combine(cefSource, "Release")))
            {
                string filename = Path.GetFileName(f);
                File.Copy(f, Path.Combine(libPath, filename));
            }
            foreach (var f in Directory.GetFiles(Path.Combine(cefSource, "Resources")))
            {
                string filename = Path.GetFileName(f);
                File.Copy(f, Path.Combine(resourcesPath, filename));
            }
            foreach (var f in Directory.GetFiles(Path.Combine(cefSource, "Resources", "locales")))
            {
                string filename = Path.GetFileName(f);
                File.Copy(f, Path.Combine(resourcesPath, "locales", filename));
            }
            File.Copy(Path.Combine(cefSource, "Resources", "icudtl.dat"), Path.Combine(libPath, "icudtl.dat"));

            writeLog("Cleaning up...");
            Directory.Delete(cefSource, true);
            File.Delete(tempFileBz);

            using (StreamWriter sw = File.CreateText(CefNetAppImpl.CefCookiePath))
            {
                sw.WriteLine("true");
            }

            writeLog("CEF installed successfully!");
        }

        private void writeLog(string message)
        {
            Log += message + Environment.NewLine;
        }
    }
}
