using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace TjMott.Writer.Model
{
    public class DbCommandHelper
    {
        public SQLiteConnection Connection { get; private set; }
        public SQLiteCommand Command { get; private set; }
        public Dictionary<string, SQLiteParameter> Parameters { get; private set; }

        public DbCommandHelper(SQLiteConnection con)
        {
            Connection = con;
            Parameters = new Dictionary<string, SQLiteParameter>();
            Command = new SQLiteCommand(Connection);
        }

        public void AddParameter(string name)
        {
            SQLiteParameter p = new SQLiteParameter();
            p.ParameterName = name;
            Parameters[name] = p;
            Command.Parameters.Add(p);
        }
    }
}
