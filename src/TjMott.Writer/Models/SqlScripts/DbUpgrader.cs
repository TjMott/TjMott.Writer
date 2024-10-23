using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace TjMott.Writer.Models.SqlScripts
{
    public abstract class DbUpgrader
    {
        public int StartVersion { get; protected set; }
        public int TargetVersion { get; protected set; }
        public abstract Task<bool> DoUpgradeAsync(SqliteConnection connection, Window dialogOwner);

        protected static async Task<bool> runScriptWithVersionCheckAsync(SqliteConnection connection, string script, int startVersion, int targetVersion)
        {
            // Version check.
            int currentVersion = await GetDbVersion(connection);
            if (currentVersion != startVersion)
                return false;

            // Run script.
            using (SqliteCommand cmd = new SqliteCommand(script, connection))
            {
                int result = await cmd.ExecuteNonQueryAsync();
            }

            // Version check.
            currentVersion = await GetDbVersion(connection);
            if (currentVersion == targetVersion)
                return true;

            return false;
        }

        protected static async Task<int> GetDbVersion(SqliteConnection connection)
        {
            using (SqliteCommand cmd = new SqliteCommand("SELECT Value FROM Metadata WHERE Key = 'DbVersion';", connection))
            {
                string versionString = (string)await cmd.ExecuteScalarAsync();
                int result;
                if (int.TryParse(versionString, out result))
                    return result;
                else
                    return -1;
            }
        }
    }
}
