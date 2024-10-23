using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace TjMott.Writer.Models.SqlScripts
{
    public class Update2to3 : DbUpgrader
    {
        private static string _script2to3 = @"

    DROP TRIGGER FlowDocument_ai;
    DROP TRIGGER FlowDocument_ad;
    DROP TRIGGER FlowDocument_au;
    DROP TABLE FlowDocument_fts;

    DROP TRIGGER MarkdownDocument_ai;
    DROP TRIGGER MarkdownDocument_ad;
    DROP TRIGGER MarkdownDocument_au;
    DROP TABLE MarkdownDocument_fts;
    DROP TABLE Ticket;
   
    DROP TRIGGER File_MarkdownDoc_ad;

    -- Remove other deprecated tables
    DROP TABLE File;
    DROP TABLE SpellcheckWord;

    -- Add new Document table (which replaces FlowDocument and MarkdownDocument)
    CREATE TABLE Document
    (
        id INTEGER PRIMARY KEY,
        UniverseId INTEGER,
        Json TEXT,
        PlainText TEXT,
        WordCount INTEGER DEFAULT 0,
        IsEncrypted INTEGER DEFAULT 0,
        DocumentType TEXT NOT NULL DEFAULT 'Manuscript',

        FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
    );

    -- Add Document FTS
    CREATE VIRTUAL TABLE Document_fts USING fts5(PlainText, Content=Document, content_rowid=id);
    CREATE TRIGGER Document_ai AFTER INSERT ON Document BEGIN
      INSERT INTO Document_fts(rowid, PlainText) VALUES (new.id, new.PlainText);
    END;
    CREATE TRIGGER Document_ad AFTER DELETE ON Document BEGIN
      INSERT INTO Document_fts(Document_fts, rowid, PlainText) VALUES ('delete', old.id, old.PlainText);
    END;
    CREATE TRIGGER Document_au AFTER UPDATE ON Document BEGIN
      INSERT INTO Document_fts(Document_fts, rowid, PlainText) VALUES ('delete', old.id, old.PlainText);
      INSERT INTO Document_fts(rowid, PlainText) VALUES (new.id, new.PlainText);
    END;

    -- Create new Category table
    DROP TRIGGER Category_MarkdownDoc_ad;
    DROP TRIGGER Category_MarkdownDoc_au;
    ALTER TABLE Category RENAME TO Category_Old;
    CREATE TABLE Category
    (
        id INTEGER PRIMARY KEY,
        UniverseId INTEGER,
        Name TEXT DEFAULT 'New Category',
        SortIndex INTEGER DEFAULT 0,

        FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
    );

    -- Create new Story table
    DROP TRIGGER Story_MarkdownDoc_ad;
    DROP TRIGGER Story_MarkdownDoc_au;
    DROP TRIGGER Story_ai;
    DROP TRIGGER Story_ad;
    DROP TRIGGER Story_au;
    DROP TABLE Story_fts;
    ALTER TABLE Story RENAME TO Story_Old;
    CREATE TABLE Story
    (
        id INTEGER PRIMARY KEY,
        UniverseId INTEGER,
        CategoryId INTEGER DEFAULT NULL,
        Name TEXT DEFAULT 'New Story',
        Subtitle TEXT DEFAULT '',
        Author TEXT DEFAULT '',
        Edition TEXT DEFAULT '',
        ISBN TEXT DEFAULT '',
        ASIN TEXT DEFAULT '',
        SortIndex INTEGER DEFAULT 0,
        CopyrightPageId INTEGER DEFAULT NULL,

        FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
        FOREIGN KEY(CategoryId) REFERENCES Category(id) ON DELETE SET NULL,
        FOREIGN KEY(CopyrightPageId) REFERENCES Document(id)
    );
    CREATE TRIGGER Story_NoteDoc_ad AFTER DELETE ON Story BEGIN
      DELETE FROM Document WHERE id = (old.CopyrightPageId);
    END;
    CREATE VIRTUAL TABLE Story_fts USING fts5(Name, content=Story, content_rowid=id);
    CREATE TRIGGER Story_ai AFTER INSERT ON Story BEGIN
      INSERT INTO Story_fts(rowid, Name) VALUES (new.id, new.Name);
    END;
    CREATE TRIGGER Story_ad AFTER DELETE ON Story BEGIN
      INSERT INTO Story_fts(Story_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
    END;
    CREATE TRIGGER Story_au AFTER UPDATE ON Story BEGIN
      INSERT INTO Story_fts(Story_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
      INSERT INTO Story_fts(rowid, Name) VALUES (new.id, new.Name);
    END;

    -- Create new Chapter table
    DROP TRIGGER Chapter_MarkdownDoc_ad;
    DROP TRIGGER Chapter_ai;
    DROP TRIGGER Chapter_ad;
    DROP TRIGGER Chapter_au;
    DROP TABLE Chapter_fts;
    ALTER TABLE Chapter RENAME TO Chapter_Old;
    CREATE TABLE Chapter
    (
        id INTEGER PRIMARY KEY,
        StoryId INTEGER,
        Name TEXT DEFAULT 'New Chapter',
        SortIndex INTEGER DEFAULT 0,

        FOREIGN KEY(StoryId) REFERENCES Story(id) ON DELETE CASCADE
    );
    CREATE VIRTUAL TABLE Chapter_fts USING fts5(Name, content=Chapter, content_rowid=id);
    CREATE TRIGGER Chapter_ai AFTER INSERT ON Chapter BEGIN
      INSERT INTO Chapter_fts(rowid, Name) VALUES (new.id, new.Name);
    END;
    CREATE TRIGGER Chapter_ad AFTER DELETE ON Chapter BEGIN
      INSERT INTO Chapter_fts(Chapter_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
    END;
    CREATE TRIGGER Chapter_au AFTER UPDATE ON Chapter BEGIN
      INSERT INTO Chapter_fts(Chapter_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
      INSERT INTO Chapter_fts(rowid, Name) VALUES (new.id, new.Name);
    END;

    -- Create new Scene table
    DROP TRIGGER Scene_ai;
    DROP TRIGGER Scene_au;
    DROP TRIGGER Scene_ad;
    DROP TRIGGER Scene_MarkdownDoc_ad;
    DROP TABLE Scene_fts;
    ALTER TABLE Scene RENAME TO Scene_Old;
    CREATE TABLE Scene
    (
        id INTEGER PRIMARY KEY,
        ChapterId INTEGER,
        Name TEXT DEFAULT 'New Scene',
        SortIndex INTEGER DEFAULT 0,
        ColorA INTEGER DEFAULT 0,
        ColorR INTEGER DEFAULT 0,
        ColorG INTEGER DEFAULT 0,
        ColorB INTEGER DEFAULT 0,
        DocumentId INTEGER NOT NULL,

        FOREIGN KEY(ChapterId) REFERENCES Chapter(id) ON DELETE CASCADE,
        FOREIGN KEY(DocumentId) REFERENCES Document(id) ON DELETE CASCADE
    );
    CREATE TRIGGER Scene_Doc_ad AFTER DELETE ON Scene BEGIN
      DELETE FROM Document WHERE id = (old.DocumentId);
    END;
    CREATE VIRTUAL TABLE Scene_fts USING fts5(Name, content=Scene, content_rowid=id);
    CREATE TRIGGER Scene_ai AFTER INSERT ON Scene BEGIN
      INSERT INTO Scene_fts(rowid, Name) VALUES (new.id, new.Name);
    END;
    CREATE TRIGGER Scene_ad AFTER DELETE ON Scene BEGIN
      INSERT INTO Scene_fts(Scene_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
    END;
    CREATE TRIGGER Scene_au AFTER UPDATE ON Scene BEGIN
      INSERT INTO Scene_fts(Scene_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
      INSERT INTO Scene_fts(rowid, Name) VALUES (new.id, new.Name);
    END;

    -- Note stuff
    CREATE TABLE NoteDocument
    (
        id INTEGER PRIMARY KEY,
        UniverseId INTEGER NOT NULL,
        DocumentId INTEGER NOT NULL,
        Name TEXT DEFAULT 'New Note',

        FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
        FOREIGN KEY(DocumentId) REFERENCES Document(id) ON DELETE CASCADE
    );

    -- Delete Document when NoteDocument is deleted.
    CREATE TRIGGER NoteDocument_ad AFTER DELETE ON NoteDocument BEGIN
      DELETE FROM NoteDocument WHERE id = (old.DocumentId);
    END;

    CREATE VIRTUAL TABLE NoteDocument_fts USING fts5(Name, Content=NoteDocument, content_rowid=id);
    CREATE TRIGGER NoteDocument_ai_fts AFTER INSERT ON NoteDocument BEGIN
      INSERT INTO NoteDocument_fts(rowid, Name) VALUES (new.id, new.Name);
    END;
    CREATE TRIGGER NoteDocument_ad_fts AFTER DELETE ON NoteDocument BEGIN
      INSERT INTO NoteDocument_fts(NoteDocument_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
    END;
    CREATE TRIGGER NoteDocument_au_fts AFTER UPDATE ON NoteDocument BEGIN
      INSERT INTO NoteDocument_fts(NoteDocument_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
      INSERT INTO NoteDocument_fts(rowid, Name) VALUES (new.id, new.Name);
    END;

    CREATE TABLE NoteCategory
    (
        id INTEGER PRIMARY KEY,
        UniverseId INTEGER NOT NULL,
        ParentId INTEGER DEFAULT NULL,
        Name TEXT DEFAULT 'New Category',

        FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
        FOREIGN KEY(ParentId) REFERENCES NoteCategory(id) ON DELETE SET NULL
    );
    CREATE TABLE NoteCategoryDocument
    (
        id INTEGER PRIMARY KEY,
        NoteDocumentId INTEGER NOT NULL,
        NoteCategoryId INTEGER NOT NULL,

        FOREIGN KEY(NoteDocumentId) REFERENCES NoteDocument(id) ON DELETE CASCADE,
        FOREIGN KEY(NoteCategoryId) REFERENCES NoteCategory(id) ON DELETE SET NULL
    );


    --

    UPDATE Metadata SET Value = 3 WHERE Key = 'DbVersion';
";



        public Update2to3()
        {
            StartVersion = 2;
            TargetVersion = 3;
        }

        public override async Task<bool> DoUpgradeAsync(SqliteConnection connection, Window dialogOwner)
        {
            bool result = await runScriptWithVersionCheckAsync(connection, _script2to3, 2, 3);
            if (!result)
                return result;

            

            return result;
        } // DoUpgradeAsync
    }
}
