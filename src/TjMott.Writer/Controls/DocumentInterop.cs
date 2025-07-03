using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TjMott.Writer.Controls
{
    /// <summary>
    /// Helper class to facilitate data marshaling between the C# app and the QuillJS app.
    /// </summary>
    public class DocumentInterop
    {
        public event EventHandler ReadyToInitialize;
        public event EventHandler EditorLoaded;
        public event EventHandler TextChanged;
        public event EventHandler SaveDocumentRequested;

        /// <summary>
        /// Plain text.
        /// </summary>
        private string _documentText = "";
        /// <summary>
        /// JSON string.
        /// </summary>
        private string _documentJson = "";

        public void RaiseReadyToInitEvent()
        {
            Dispatcher.UIThread.Post(() =>
            {
                ReadyToInitialize?.Invoke(this, EventArgs.Empty);
            });
        }

        public void RaiseEditorLoadedEvent()
        {
            Dispatcher.UIThread.Post(() =>
            {
                EditorLoaded?.Invoke(this, EventArgs.Empty);
            });
        }

        public void RaiseTextChangedEvent()
        {
            Dispatcher.UIThread.Post(() =>
            {
                TextChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        public void RaiseSaveDocumentRequestedEvent()
        {
            Dispatcher.UIThread.Post(() =>
            {
                SaveDocumentRequested?.Invoke(this, EventArgs.Empty);
            });
        }

        public void SetDocumentJson(string newText)
        {
            if (_documentJson != newText)
            {
                _documentJson = newText;
            }
        }

        public void SetDocumentText(string newText)
        {
            if (_documentText != newText)
            {
                _documentText = newText;
            }
        }

        public string GetDocumentText()
        {
            return _documentText;
        }

        public string GetDocumentJson()
        {
            return _documentJson;
        }

        public static Task<object> AsyncCallNativeMethod(Func<object> nativeMethod)
        {
            return Task.Run(() =>
            {
                var result = nativeMethod.Invoke();
                if (result is Task task)
                {
                    if (task.GetType().IsGenericType)
                    {
                        return ((dynamic)task).Result;
                    }

                    return task;
                }

                return result;
            });
        }
    }
}
