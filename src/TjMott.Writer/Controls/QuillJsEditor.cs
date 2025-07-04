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

            public AssetHash(string filename, string expectedHash)
            {
                FileName = filename;
                ExpectedHash = expectedHash;
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
            BrowserInitialized += QuillJsEditor_BrowserInitialized;
            JavascriptUncaughException += QuillJsEditor_JavascriptUncaughException;
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

        public static async Task<bool> VerifyHashes()
        {
            // The Quill HTML/CSS/JS files would be an easy target for malware. So check the SHA256 hash
            // on all of these and issue a warning if something doesn't match.
            AssetHashes.Clear();
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "editor.html"), "1714613E331074CF514C9AF20334F98360FF2BED91B3EB0A818BB82BEC3230EA"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.bubble.css"), "E8E966FBBE4848DABF3C2B7E2C899A78659FEF4FF742420DA8FE38CD5C219238"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.core.css"), "EDDF53780BF28A315F65A6846FEEB4CA82E62E06A74B9462152F4D87AD8D3BC4"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.core.js"), "6D4B501DF20B89117FB88294E8BD85813B254FF6A10184949A28DEF97F294C0B"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.js"), "F11DE2930B4C68D015625D63EBDCC25CDB7B3B75A2BB2364DBC10597404BCC03"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.min.js"), "C8AB352E074C23CFE6C83DB82D3BAF3A859038D8CD86D8B3960D6EEAD6DC5813"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.min.js.map"), "BE1D65DA4425B26396C70033141DCBB88D8304738AE67D8625B9920E3B74B02B"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.snow.css"), "137D947401EA1CA7F1954F4F6633F25EB7B0501E3E2AB3570921FAFDDADBC3DD"));
            AssetHashes.Add(new AssetHash(Path.Join("Assets", "quilljs", "quill.snow.dark.css"), "99D53F8F263BD0248C125B64B20A8F39B0E4B2788657B8B7587EF669A1069C15"));

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
