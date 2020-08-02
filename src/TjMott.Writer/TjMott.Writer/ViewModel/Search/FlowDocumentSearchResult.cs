using System;
using System.Data.SQLite;
using System.Windows.Input;
using TjMott.Writer.Windows;

namespace TjMott.Writer.ViewModel.Search
{
    public class FlowDocumentSearchResult : ViewModelBase
    {

        #region ICommands
        private ICommand _openEditorCommand;
        public ICommand OpenEditorCommand
        {
            get
            {
                if (_openEditorCommand == null)
                {
                    _openEditorCommand = new RelayCommand(param => OpenEditor(), param => CanOpenEditor());
                }
                return _openEditorCommand;
            }
        }
        #endregion

        #region Properties
        public string StoryName { get; set; }
        public string ChapterName { get; set; }
        public string SceneName { get; set; }
        public ViewModelBase Owner { get; set; }
        public long rowid { get; set; }
        public string Snippet { get; set; }
        public string SnippetPre { get; set; }
        public string SnippetResult { get; set; }
        public string SnippetPost { get; set; }
        #endregion

        public FlowDocumentSearchResult(SQLiteDataReader sqlReader)
        {
            rowid = sqlReader.GetInt64(0);
            Snippet = sqlReader.GetString(1);

            int resultStart = Snippet.IndexOf("<FTSSearchResult>");
            int resultEnd = Snippet.IndexOf("</FTSSearchResult>");
            SnippetPre = SearchViewModel.ProcessSnippet(Snippet.Substring(0, resultStart));
            SnippetResult = SearchViewModel.ProcessSnippet(Snippet.Substring(resultStart, resultEnd - resultStart));
            SnippetPost = SearchViewModel.ProcessSnippet(Snippet.Substring(resultEnd));
        }

        public void OpenEditor()
        {
            if (Owner is SceneViewModel)
            {
                SceneViewModel vm = Owner as SceneViewModel;
                FlowDocumentEditorWindow.ShowEditorWindow(vm.Model.FlowDocumentId, vm.Model.Connection, vm.ChapterVm.StoryVm.UniverseVm.SpellcheckDictionary, string.Format("Scene: {0}", vm.Model.Name), SnippetResult);
            }
        }

        public bool CanOpenEditor()
        {
            return Owner is SceneViewModel;
        }
    }
}
