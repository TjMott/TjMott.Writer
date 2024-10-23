using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace TjMott.Writer.Models.SqlScripts
{
    public class Update1to2 : DbUpgrader
    {
        private static string _script = @"
    DROP TRIGGER Chapter_ad;
    CREATE TRIGGER Chapter_ad AFTER DELETE ON Chapter BEGIN
      INSERT INTO Chapter_fts(Chapter_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
    END;

    DROP TRIGGER Story_ad;
    CREATE TRIGGER Story_ad AFTER DELETE ON Story BEGIN
      INSERT INTO Story_fts(Story_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
    END;

    UPDATE Metadata SET Value = 2 WHERE Key = 'DbVersion';
";
        public Update1to2()
        {
            StartVersion = 1;
            TargetVersion = 2;
        }

        public override Task<bool> DoUpgradeAsync(SqliteConnection connection, Window dialogOwner)
        {
            return runScriptWithVersionCheckAsync(connection, _script, StartVersion, TargetVersion);
        }
    }
}
