using System;
using System.Collections.Generic;
using System.Data.SQLite;
using TjMott.Writer.Model.Attributes;

namespace TjMott.Writer.Model.SQLiteClasses
{
    [DbTableName("SpellcheckWord")]
    public class SpellcheckWord : IDbType
    {
        #region Database Properties
        [DbField]
        public long id { get; set; }
        [DbField]
        public long UniverseId { get; set; }
        [DbField]
        public string Word { get; set; }
        #endregion

        public SQLiteConnection Connection { get; set; }
        private static DbHelper<SpellcheckWord> _dbHelper;

        public SpellcheckWord(SQLiteConnection connection)
        {
            Connection = connection;
            if (_dbHelper == null)
                _dbHelper = new DbHelper<SpellcheckWord>(Connection);

            id = -1;
            Word = "";
        }

        public void Create()
        {
            _dbHelper.Insert(this);
            Load();
        }

        public void Delete()
        {
            _dbHelper.Delete(this);
        }

        public void Load()
        {
            _dbHelper.Load(this);
        }

        public void Save()
        {
            _dbHelper.Update(this);
        }

        public static List<SpellcheckWord> GetAllSpellcheckWord(SQLiteConnection connection)
        {
            if (_dbHelper == null)
                _dbHelper = new DbHelper<SpellcheckWord>(connection);

            List<SpellcheckWord> retval = new List<SpellcheckWord>();
            List<long> ids = _dbHelper.GetAllIds();
            foreach (long id in ids)
            {
                SpellcheckWord word = new SpellcheckWord(connection);
                word.id = id;
                word.Load();
                retval.Add(word);
            }
            return retval;
        }
    }
}
