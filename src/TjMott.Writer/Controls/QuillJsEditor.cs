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

            AssetHashes.Add(new AssetHash("D4C5E6989B9F174C66512E493CB54FA0C3BC0F375791ED7C0685B1FB371D4D0D", "Assets", "editor.html"));
            AssetHashes.Add(new AssetHash("AECF8DFF9EAA4EA0CD68041366FBBE2AB98C7E84F5EE43906BB3073BFB03646B", "Assets", "quilljs", "quill.bubble.css"));
            AssetHashes.Add(new AssetHash("0FDAFD0AE4FAAE9E835F8FBACA1BE406CAB134867C69006452E32F7ADAC73199", "Assets", "quilljs", "quill.bubble.css.map"));
            AssetHashes.Add(new AssetHash("1AD491E90881EAAE64CDBBC87E82613C98350907E89C00E77FAA4316D92098E5", "Assets", "quilljs", "quill.core.css"));
            AssetHashes.Add(new AssetHash("AE0D05BAAB887F9AF56D5912EB00BB7ABB31D05D281383F6912F853AD361506D", "Assets", "quilljs", "quill.core.css.map"));
            AssetHashes.Add(new AssetHash("6A2E72F1FCDFB6A0996AE275FEE8580E912ADE8A6DF92BD788DAE8D2F6FBBBC6", "Assets", "quilljs", "quill.core.js"));
            AssetHashes.Add(new AssetHash("85B256D9E22A9605D18C559782EFA47F989418AE343CC72A1799E9DE345BDE6F", "Assets", "quilljs", "quill.core.js.map"));
            AssetHashes.Add(new AssetHash("F6157C72AC9B3F51CDEAD426335688A027B12405D9D6A4DAADD38A676B2D7FF2", "Assets", "quilljs", "quill.js"));
            AssetHashes.Add(new AssetHash("A508052334C1C1171D529DA80B19948F0521C5CC1DB341833908A25A10A2722F", "Assets", "quilljs", "quill.js.map"));
            AssetHashes.Add(new AssetHash("1C7948CD13AA92FAC6390319BC1E5E461823DA171519D3A768DB56164F871636", "Assets", "quilljs", "quill.snow.css"));
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
