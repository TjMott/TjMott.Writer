﻿using CefNet.Avalonia;
using CefNet.JSInterop;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            BrowserCreated += CustomWebView_BrowserCreated;
        }

        public async Task<string> GetText()
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            string text = quill.getText();
            return text;
        }

        public async Task<string> GetJsonDocument()
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = quill.getContents();
            return delta;
        }

        public async Task<string> GetJsonText()
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = quill.getContents();
            string contents = window.JSON.stringify(delta, null, 4);
            return contents;
        }

        public async void SetJsonDocument(dynamic json)
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            quill.setContents(json);
        }

        public async void SetJsonText(string json)
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;
            dynamic quill = window.editor;
            dynamic delta = window.JSON.parse(json);
            quill.setContents(delta);
        }

        public async Task<int> GetWordCount()
        {
            string text = await GetText();
            string[] words = text.Split(new string[] { " ", "\r", "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return words.Length;
        }

        private void CustomWebView_BrowserCreated(object sender, EventArgs e)
        {
            BrowserCreated -= CustomWebView_BrowserCreated;
            initEditor();
        }

        private async void initEditor()
        {
            dynamic scriptableObject = await GetMainFrame().GetScriptableObjectAsync(CancellationToken.None).ConfigureAwait(false);
            dynamic window = scriptableObject.window;

            // Enumerate installed fonts, add them to the Quill editor.
            using (InstalledFontCollection col = new InstalledFontCollection())
            {
                foreach (var font in col.Families)
                {
                    window.addFont(font.Name);
                }
            }

            // Initialize Quilljs.
            window.initEditor();
        }
    }
}
