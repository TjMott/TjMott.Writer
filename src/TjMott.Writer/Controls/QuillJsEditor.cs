using CefNet.Avalonia;
using System.IO;

namespace TjMott.Writer.Controls
{
    public class QuillJsEditor : WebView
    {
       
        private static string editor_path;


        static QuillJsEditor()
        {
            editor_path = "file:///" + Path.Join(Directory.GetCurrentDirectory(), "Assets", "editor.html");
        }

        public QuillJsEditor()
        {
            InitialUrl = editor_path;
        }
    }
}
