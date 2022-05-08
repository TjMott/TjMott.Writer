using System;
using Microsoft.Data.Sqlite;

namespace TjMott.Writer.Models
{
    public interface IDbType
    {
        long id { get; set; }
        SqliteConnection Connection { get; set; }
        void Load();
        void Create();
        void Save();
        void Delete();
    }
}
