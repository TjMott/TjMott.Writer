using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Text;

namespace TjMott.Writer.Models.SQLiteClasses
{
    public class InvalidMetadataKeyException : ApplicationException
    {
        public string Key { get; private set; }
        public InvalidMetadataKeyException(string key, string message) : base(message)
        {
            Key = key;
        }
    }

    public class Metadata
    {

        public const int ExpectedVersion = 4;

        private SqliteConnection _connection;

        private const string DBVERSION_KEY = "DbVersion";
        private const string DEFAULT_UNIVERSE_KEY = "DefaultUniverse";

        private DbCommandHelper _selectKeyCommand;
        private DbCommandHelper _setKeyCommand;

        public Metadata(SqliteConnection connection)
        {
            _connection = connection;

            _selectKeyCommand = new DbCommandHelper(_connection);
            _selectKeyCommand.Command.CommandText = "SELECT Value FROM Metadata WHERE Key = @key;";
            _selectKeyCommand.AddParameter("@key");

            _setKeyCommand = new DbCommandHelper(_connection);
            _setKeyCommand.Command.CommandText = "UPDATE Metadata SET Value = @value WHERE Key = @key;";
            _setKeyCommand.AddParameter("@key");
            _setKeyCommand.AddParameter("@value");
        }

        private string getValue(string key)
        {
            _selectKeyCommand.Parameters["@key"].Value = key;
            using (SqliteDataReader reader = _selectKeyCommand.Command.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    throw new InvalidMetadataKeyException(key, string.Format("Invalid metadata key '{0}'.", key));
                }
                reader.Read();
                return reader.GetString(0);
            }
        }

        private void setValue(string key, string value)
        {
            _setKeyCommand.Parameters["@key"].Value = key;
            _setKeyCommand.Parameters["@value"].Value = value;
            int affectedRows = _setKeyCommand.Command.ExecuteNonQuery();
            if (affectedRows != 1)
            {
                throw new InvalidMetadataKeyException(key, string.Format("Failed to update metadata key '{0}'.", key));
            }
        }

        public int DbVersion
        {
            get { return int.Parse(getValue(DBVERSION_KEY)); }
        }

        public long DefaultUniverseId
        {
            get { return long.Parse(getValue(DEFAULT_UNIVERSE_KEY)); }
            set { setValue(DEFAULT_UNIVERSE_KEY, value.ToString()); }
        }
    }
}
