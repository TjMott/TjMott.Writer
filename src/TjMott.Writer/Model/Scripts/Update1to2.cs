using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TjMott.Writer.Model.Scripts
{
    public static class Update1to2
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

        public static string Script
        {
            get { return _script; }
        }
    }
}
