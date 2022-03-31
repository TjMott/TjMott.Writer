using System;

namespace TjMott.Writer.Model.Scripts
{
    public static class DbInitScript
    {
        private static string _init = @"

CREATE TABLE Metadata
(
    id INTEGER PRIMARY KEY,
    Key TEXT UNIQUE NOT NULL,
    Value TEXT NOT NULL
);

INSERT INTO Metadata(Key, Value) VALUES ('DbVersion', 2);
INSERT INTO Metadata(Key, Value) VALUES ('DefaultUniverse', 0);

CREATE TABLE Universe
(
    id INTEGER PRIMARY KEY,
    Name TEXT DEFAULT 'New Universe',
    SortIndex INTEGER DEFAULT 0,
    MarkdownCss TEXT DEFAULT '',
    DefaultTemplateId INTEGER DEFAULT 0
);

CREATE TABLE FlowDocument
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Xml TEXT,
    PlainText TEXT,
    WordCount INTEGER DEFAULT 0,
    IsEncrypted INTEGER DEFAULT 0,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE MarkdownDocument
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    MarkdownText TEXT DEFAULT '',
    PlainText TEXT DEFAULT '',
    Name TEXT DEFAULT 'New Document',

    -- If IsSpecial is true, this is attached to an item (ticket, scene, chapter, etc.). If false, it's on its own.
    IsSpecial INTEGER DEFAULT 0,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE MarkdownCategory
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    ParentId INTEGER,
    Name TEXT DEFAULT 'New Category',

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(ParentId) REFERENCES MarkdownCategory(id) ON DELETE SET NULL
);

CREATE TABLE MarkdownCategoryDocument
(
    id INTEGER PRIMARY KEY,
    MarkdownCategoryId INTEGER,
    MarkdownDocumentId INTEGER,

    FOREIGN KEY(MarkdownCategoryId) REFERENCES MarkdownCategory(id) ON DELETE CASCADE,
    FOREIGN KEY(MarkdownDocumentId) REFERENCES MarkdownDocument(id) ON DELETE CASCADE
);

CREATE TABLE SpellcheckWord
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Word TEXT,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE Category
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Name TEXT DEFAULT 'New Category',
    SortIndex INTEGER DEFAULT 0,
    MarkdownDocumentId INTEGER DEFAULT NULL,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(MarkdownDocumentId) REFERENCES MarkdownDocument(id)
);

-- Delete MarkdownDocument after its category is deleted.
CREATE TRIGGER Category_MarkdownDoc_ad AFTER DELETE ON Category BEGIN
  DELETE FROM MarkdownDocument WHERE id = (old.MarkdownDocumentId);
END;

-- Update MarkdownDocument when category is renamed.
CREATE TRIGGER Category_MarkdownDoc_au AFTER UPDATE ON Category BEGIN
  UPDATE MarkdownDocument
  SET Name = new.Name
  WHERE id = new.id;
END;

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
    MarkdownDocumentId INTEGER DEFAULT NULL,
    FlowDocumentId INTEGER DEFAULT NULL,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(FlowDocumentId) REFERENCES FlowDocument(id),
    FOREIGN KEY(CategoryId) REFERENCES Category(id) ON DELETE SET NULL,
    FOREIGN KEY(MarkdownDocumentId) REFERENCES MarkdownDocument(id)
);

-- Delete MarkdownDocument after its Story is deleted.
CREATE TRIGGER Story_MarkdownDoc_ad AFTER DELETE ON Story BEGIN
  DELETE FROM MarkdownDocument WHERE id = (old.MarkdownDocumentId);
END;

-- Delete FlowDocument after its Story is deleted.
CREATE TRIGGER Story_FlowDoc_ad AFTER DELETE ON Story BEGIN
  DELETE FROM FlowDocument WHERE id = (old.FlowDocumentId);
END;

-- Update MarkdownDocument when category is renamed.
CREATE TRIGGER Story_MarkdownDoc_au AFTER UPDATE ON Story BEGIN
  UPDATE MarkdownDocument
  SET Name = new.Name
  WHERE id = new.id;
END;

CREATE TABLE File
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Name TEXT DEFAULT 'New File',
    FileName TEXT DEFAULT '',
    FileType TEXT DEFAULT '',
    Data BLOB,
    SortIndex INTEGER DEFAULT 0,
    MarkdownDocumentId INTEGER DEFAULT NULL,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(MarkdownDocumentId) REFERENCES MarkdownDocument(id) ON DELETE CASCADE
);

-- Delete MarkdownDocument after its file is deleted.
CREATE TRIGGER File_MarkdownDoc_ad AFTER DELETE ON File BEGIN
  DELETE FROM MarkdownDocument WHERE id = (old.MarkdownDocumentId);
END;

CREATE TABLE Chapter
(
    id INTEGER PRIMARY KEY,
    StoryId INTEGER,
    Name TEXT DEFAULT 'New Chapter',
    SortIndex INTEGER DEFAULT 0,
    MarkdownDocumentId INTEGER DEFAULT NULL,

    FOREIGN KEY(StoryId) REFERENCES Story(id) ON DELETE CASCADE,
    FOREIGN KEY(MarkdownDocumentId) REFERENCES MarkdownDocument(id)
);

-- Delete MarkdownDocument after its chapter is deleted.
CREATE TRIGGER Chapter_MarkdownDoc_ad AFTER DELETE ON Chapter BEGIN
  DELETE FROM MarkdownDocument WHERE id = (old.MarkdownDocumentId);
END;

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
    FlowDocumentId INTEGER DEFAULT NULL,
    MarkdownDocumentId INTEGER DEFAULT NULL,

    FOREIGN KEY(ChapterId) REFERENCES Chapter(id) ON DELETE CASCADE,
    FOREIGN KEY(FlowDocumentId) REFERENCES FlowDocument(id),
    FOREIGN KEY(MarkdownDocumentId) REFERENCES MarkdownDocument(id)
);

-- Delete FlowDocument after its scene is deleted.
CREATE TRIGGER Scene_FlowDoc_ad AFTER DELETE ON Scene BEGIN
  DELETE FROM FlowDocument WHERE id = (old.FlowDocumentId);
END;

-- Delete MarkdownDocument after its scene is deleted.
CREATE TRIGGER Scene_MarkdownDoc_ad AFTER DELETE ON Scene BEGIN
  DELETE FROM MarkdownDocument WHERE id = (old.MarkdownDocumentId);
END;

CREATE TABLE Ticket
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Priority INTEGER DEFAULT 2,
    Name TEXT DEFAULT 'New Ticket',
    Status TEXT DEFAULT 'Not Started',
    MarkdownDocumentId INTEGER DEFAULT NULL,
    DueDate TEXT DEFAULT '',

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(MarkdownDocumentId) REFERENCES MarkdownDocument(id)
);

-- Delete MarkdownDocument after its ticket is deleted.
CREATE TRIGGER Ticket_MarkdownDoc_ad AFTER DELETE ON Ticket BEGIN
  DELETE FROM MarkdownDocument WHERE id = (old.MarkdownDocumentId);
END;

-- Full-Text Search stuff
CREATE VIRTUAL TABLE Scene_fts USING fts5(Name, content=Scene, content_rowid=id);
-- Table rebuild, just here for reference
-- INSERT INTO Scene_fts(Scene_fts) VALUES ('rebuild');
-- Triggers to keep Scene_fts up-to-date
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

CREATE VIRTUAL TABLE Chapter_fts USING fts5(Name, content=Chapter, content_rowid=id);
-- Table rebuild, just here for reference
-- INSERT INTO Chapter_fts(Chapter_fts) VALUES ('rebuild');
-- Triggers to keep Chapter_fts up-to-date
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

CREATE VIRTUAL TABLE Story_fts USING fts5(Name, content=Story, content_rowid=id);
-- Table rebuild, just here for reference
-- INSERT INTO Story_fts(Story_fts) VALUES ('rebuild');
-- Triggers to keep Story_fts up-to-date
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




CREATE VIRTUAL TABLE FlowDocument_fts USING fts5(PlainText, content=FlowDocument, content_rowid=id);
-- Table rebuild, just here for reference
-- INSERT INTO FlowDocument_fts(FlowDocument_fts) VALUES ('rebuild');
-- Triggers to keep FlowDocument_fts up-to-date
CREATE TRIGGER FlowDocument_ai AFTER INSERT ON FlowDocument BEGIN
  INSERT INTO FlowDocument_fts(rowid, PlainText) VALUES (new.id, new.PlainText);
END;
CREATE TRIGGER FlowDocument_ad AFTER DELETE ON FlowDocument BEGIN
  INSERT INTO FlowDocument_fts(FlowDocument_fts, rowid, PlainText) VALUES ('delete', old.id, old.PlainText);
END;
CREATE TRIGGER FlowDocument_au AFTER UPDATE ON FlowDocument BEGIN
  INSERT INTO FlowDocument_fts(FlowDocument_fts, rowid, PlainText) VALUES ('delete', old.id, old.PlainText);
  INSERT INTO FlowDocument_fts(rowid, PlainText) VALUES (new.id, new.PlainText);
END;

CREATE VIRTUAL TABLE MarkdownDocument_fts USING fts5(PlainText, content=MarkdownDocument, content_rowid=id);
-- Table rebuild, just here for reference
-- INSERT INTO MarkdownDocument_fts(MarkdownDocument_fts) VALUES ('rebuild');
-- Triggers to keep MarkdownDocument_fts up-to-date
CREATE TRIGGER MarkdownDocument_ai AFTER INSERT ON MarkdownDocument BEGIN
  INSERT INTO MarkdownDocument_fts(rowid, PlainText) VALUES (new.id, new.PlainText);
END;
CREATE TRIGGER MarkdownDocument_ad AFTER DELETE ON MarkdownDocument BEGIN
  INSERT INTO MarkdownDocument_fts(MarkdownDocument_fts, rowid, PlainText) VALUES ('delete', old.id, old.PlainText);
END;
CREATE TRIGGER MarkdownDocument_au AFTER UPDATE ON MarkdownDocument BEGIN
  INSERT INTO MarkdownDocument_fts(MarkdownDocument_fts, rowid, PlainText) VALUES ('delete', old.id, old.PlainText);
  INSERT INTO MarkdownDocument_fts(rowid, PlainText) VALUES (new.id, new.PlainText);
END;

";
        public static string InitScript
        {
            get
            {
                return _init;
            }
        }
    }
}
