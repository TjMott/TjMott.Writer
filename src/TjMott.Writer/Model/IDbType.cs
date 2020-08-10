using System;
using System.Data.SQLite;

namespace TjMott.Writer.Model
{
    public interface IDbType
    {
        long id { get; set; }
        SQLiteConnection Connection { get; set; }
        void Load();
        void Create();
        void Save();
        void Delete();
    }
}
