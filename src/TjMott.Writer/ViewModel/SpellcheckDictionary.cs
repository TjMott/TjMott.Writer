using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class SpellcheckDictionary : ViewModelBase
    {
        public event EventHandler DictionaryModified;

        private void onDictionaryModified()
        {
            if (DictionaryModified != null)
                DictionaryModified(this, new EventArgs());
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

        public void LoadFromDatabase()
        {
            Words.Clear();
            _tempFile = getDictionaryFileName();
            var words = SpellcheckWord.GetAllSpellcheckWord(UniverseVm.Model.Connection);

            foreach (var w in words)
            {
                if (w.UniverseId == UniverseVm.Model.id)
                    Words.Add(w);
            }
        }
    }
}
