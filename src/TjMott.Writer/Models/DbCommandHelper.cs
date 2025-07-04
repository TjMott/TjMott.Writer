using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;

namespace TjMott.Writer.Models
{
    public class DbCommandHelper : IDisposable
    {
        private static Regex _paramRegex = new Regex("(@[\\w]+)", RegexOptions.Compiled);
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

        public void SetParameterizedQuery(string query)
        {
            Command.CommandText = query;
            MatchCollection matches = _paramRegex.Matches(query);
            foreach (Match match in matches)
            {
                AddParameter(match.Value);
            }
        }

        public void Dispose()
        {
            Parameters.Clear();
            if (Command != null)
            {
                Command.Dispose();
                Command = null;
            }
        }
    }
}
