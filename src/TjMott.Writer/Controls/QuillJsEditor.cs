using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xilium.CefGlue.Avalonia;

namespace TjMott.Writer.Controls
{
    /// <summary>
    /// Extends AvaloniaCefBrowser from the CefGlue project.
    /// </summary>
    public class QuillJsEditor : AvaloniaCefBrowser
    {
        public class AssetHash
        {
            public string FileName { get; private set; }
            public string ExpectedHash { get; private set; }
            public string ActualHash { get; private set; }

            public ReadOnlyCollection<string> FilePathInfo { get; private set; }

            public AssetHash(string expectedHash, params string[] pathInfo)
            {
                FileName = Path.Join(pathInfo);
                ExpectedHash = expectedHash;

                FilePathInfo = new ReadOnlyCollection<string>(pathInfo);
            }

            public void CheckHash()
            {
                using (SHA256 sha = SHA256.Create())
                {
                    using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), FileName)))
                    {
                        byte[] hash = sha.ComputeHash(fs);
                        ActualHash = BitConverter.ToString(hash).Replace("-", "");
                    }
                }
            }
        }
        private static string editor_path;


        static QuillJsEditor()
        {
            editor_path = "file:///" + Path.Join(Directory.GetCurrentDirectory(), "Assets", "editor.html");
            AssetHashes = new ObservableCollection<AssetHash>();
        }

        public QuillJsEditor()
        {
            Address = editor_path;
            //BrowserInitialized += QuillJsEditor_BrowserInitialized;
            //JavascriptUncaughException += QuillJsEditor_JavascriptUncaughException;
        }

        private void QuillJsEditor_JavascriptUncaughException(object sender, Xilium.CefGlue.Common.Events.JavascriptUncaughtExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"QuillJsEditor exception: {e.Message}");
        }

        private void QuillJsEditor_BrowserInitialized()
        {
            System.Diagnostics.Debug.WriteLine("Browser initialized.");
        }


        public static ObservableCollection<AssetHash> AssetHashes { get; private set; }

        public static async Task CopyHashesToClipboardAsync(Window window)
        {
            string hashes = "";

            // Create code to paste into the VerifyHashes function.
            foreach (var hash in AssetHashes)
            {
                hashes += $"AssetHashes.Add(new AssetHash(\"{hash.ActualHash}\",";
                for (int i = 0; i < hash.FilePathInfo.Count; i++)
                {
                    hashes += $" \"{hash.FilePathInfo[i]}\"";
                    if (i < hash.FilePathInfo.Count - 1)
                        hashes += ",";
                }
                hashes += "));" + Environment.NewLine;
            }

            await window.Clipboard.SetTextAsync(hashes);
        }

        public static async Task<bool> VerifyHashes()
        {
            // The Quill HTML/CSS/JS files would be an easy target for malware. So check the SHA256 hash
            // on all of these and issue a warning if something doesn't match.
            AssetHashes.Clear();

            AssetHashes.Add(new AssetHash("12BD53EC55730B0D9C8CF79800B3F3F060CAC2016E0DE5005954A67E6F73D5F5", "Assets", "editor.html"));
            AssetHashes.Add(new AssetHash("9E6D61ADD4615AD9166389ED0CBB5F0F5B59D37EE37284FC577595857BEAA1AF", "Assets", "quilljs", "quill.bubble.css"));
            AssetHashes.Add(new AssetHash("0FDAFD0AE4FAAE9E835F8FBACA1BE406CAB134867C69006452E32F7ADAC73199", "Assets", "quilljs", "quill.bubble.css.map"));
            AssetHashes.Add(new AssetHash("904BC913857BB1311218A90570886991BF958D979E52C6699D1175C1D4B48236", "Assets", "quilljs", "quill.core.css"));
            AssetHashes.Add(new AssetHash("AE0D05BAAB887F9AF56D5912EB00BB7ABB31D05D281383F6912F853AD361506D", "Assets", "quilljs", "quill.core.css.map"));
            AssetHashes.Add(new AssetHash("FA21E0DB45052711B5A6E2EB6F58227967C78E3575095037AFDB4E58D437191E", "Assets", "quilljs", "quill.core.js"));
            AssetHashes.Add(new AssetHash("E988220161BAFD5639939AA31B3AEE47B797B587F1FC9AC580048348289DBEB2", "Assets", "quilljs", "quill.core.js.map"));
            AssetHashes.Add(new AssetHash("E6AEEE50E3A5AFCF08A39621424DE581872807F3F7FB9030AB595FD2B45AEFE6", "Assets", "quilljs", "quill.js"));
            AssetHashes.Add(new AssetHash("CDF628219497E844DDE724B26F08E4917AC0FBAE8FB89EFDA6006C9B8460E8D9", "Assets", "quilljs", "quill.js.map"));
            AssetHashes.Add(new AssetHash("6005AC521F488A6ADEC4F1AC36E8EE8BD4985AA9EBD14AD7084270F1B64282D7", "Assets", "quilljs", "quill.snow.css"));
            AssetHashes.Add(new AssetHash("AAC83B00C50D7396614BB8B37C0A32A549242322C558BA95E6B1A51EE3FB6935", "Assets", "quilljs", "quill.snow.css.map"));
            AssetHashes.Add(new AssetHash("99D53F8F263BD0248C125B64B20A8F39B0E4B2788657B8B7587EF669A1069C15", "Assets", "quilljs", "quill.snow.dark.css"));


            Task[] hashTasks = new Task[AssetHashes.Count];
            for (int i = 0; i < AssetHashes.Count; i++)
            {
                hashTasks[i] = Task.Run(AssetHashes[i].CheckHash);
            }
            await Task.WhenAll(hashTasks);
            foreach (var asset in AssetHashes)
            {
                if (asset.ExpectedHash != asset.ActualHash)
                    return false;
            }
            return true;
        }
    }
}
