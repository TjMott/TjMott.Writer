using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Model.SQLiteClasses;
using TjMott.Writer.Windows;

namespace TjMott.Writer.ViewModel
{
    public class SpellcheckDictionary : ViewModelBase
    {
        public event EventHandler DictionaryModified;

        #region ICommands
        private ICommand _editDictionaryCommand;
        public ICommand EditDictionaryCommand
        {
            get
            {
                if (_editDictionaryCommand == null)
                {
                    _editDictionaryCommand = new RelayCommand(param => EditDictionary());
                }
                return _editDictionaryCommand;
            }
        }

        private ICommand _deleteWordCommand;
        public ICommand DeleteWordCommand
        {
            get
            {
                if (_deleteWordCommand == null)
                {
                    _deleteWordCommand = new RelayCommand(param => DeleteSelectedWord(), param => CanDeleteSelectedWord());
                }
                return _deleteWordCommand;
            }
        }
        #endregion

        private void onDictionaryModified()
        {
            if (DictionaryModified != null)
                DictionaryModified(this, new EventArgs());
        }

        private SpellcheckWord _selectedWord;
        public SpellcheckWord SelectedWord
        {
            get { return _selectedWord; }
            set
            {
                _selectedWord = value;
                OnPropertyChanged("SelectedWord");
            }
        }

        private string _tempFile;
        public UniverseViewModel UniverseVm { get; private set; }
        public BindingList<SpellcheckWord> Words { get; private set; }

        public SpellcheckDictionary(UniverseViewModel universe)
        {
            Words = new BindingList<SpellcheckWord>();
            UniverseVm = universe;
            LoadFromDatabase();
        }

        private string getDictionaryFileName()
        {
            string directoryPath = Path.GetTempPath();
            string dictionaryName = UniverseVm.Model.Name + "_dictionary.lex";
            string dictionaryPath = string.Format("{0}\\{1}", directoryPath, dictionaryName);
            return dictionaryPath;
        }

        public Uri GetDictionaryUri()
        {
            // If dictionary doesn't exist, create it
            // and load with data from the XML file.
            if (!File.Exists(_tempFile))
            {
                using (StreamWriter writer = File.CreateText(_tempFile))
                {
                    foreach (var word in Words)
                    {
                        writer.WriteLine(word.Word);
                    }
                }
            }

            return new Uri(_tempFile);
        }

        public void AddToDictionary(string entry)
        {
            foreach (var item in Words)
            {
                if (item.Word.Equals(entry))
                    return;
            }
            SpellcheckWord newItem = new SpellcheckWord(UniverseVm.Model.Connection);
            newItem.UniverseId = UniverseVm.Model.id;
            newItem.Word = entry;
            newItem.Create();
            Words.Add(newItem);

            if (File.Exists(_tempFile))
            {
                string[] array = new string[1];
                array[0] = entry;
                File.AppendAllLines(_tempFile, array);
            }
            else
            {
                GetDictionaryUri();
            }
            onDictionaryModified();
        }

        public void Close()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }

        public void RemoveDuplicates()
        {
            // Not sure how I got duplicates. Import / conversion error from the old file format?
            List<string> foundWords = new List<string>();

            for (int i = Words.Count - 1; i >= 0; i--)
            {
                SpellcheckWord w = Words[i];
                if (foundWords.Contains(w.Word))
                {
                    w.Delete();
                    Words.Remove(w);
                }
                else
                {
                    foundWords.Add(w.Word);
                }
            }

        }

        public void LoadFromDatabase()
        {
            Words.Clear();
            _tempFile = getDictionaryFileName();
            var words = SpellcheckWord.GetAllSpellcheckWord(UniverseVm.Model.Connection).OrderBy(i => i.Word);

            foreach (var w in words)
            {
                if (w.UniverseId == UniverseVm.Model.id)
                {
                    Words.Add(w);
                }
            }
        }

        public void EditDictionary()
        {
            SpellcheckDictionaryWindow.ShowSpellcheckDictionaryWindow(this);
        }

        public void DeleteSelectedWord()
        {
            SelectedWord.Delete();
            Words.Remove(SelectedWord);
            SelectedWord = null;
        }

        public bool CanDeleteSelectedWord()
        {
            return _selectedWord != null;
        }
    }
}
