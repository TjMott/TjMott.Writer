using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text;

namespace TjMott.Writer.Models
{
    public class DbCommandHelper
    {
        public SqliteConnection Connection { get; private set; }
        public SqliteCommand Command { get; private set; }
        public Dictionary<string, SqliteParameter> Parameters { get; private set; }

        public DbCommandHelper(SqliteConnection con)
        {
            Connection = con;
            Parameters = new Dictionary<string, SqliteParameter>();
            Command = new SqliteCommand(null, Connection);
        }

        public void AddParameter(string name)
        {
            SqliteParameter p = new SqliteParameter();
            p.ParameterName = name;
            Parameters[name] = p;
            Command.Parameters.Add(p);
        }
    }
}
