using System;

namespace TjMott.Writer.Models.SqlScripts
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

INSERT INTO Metadata(Key, Value) VALUES ('DbVersion', 4);
INSERT INTO Metadata(Key, Value) VALUES ('DefaultUniverse', 0);

CREATE TABLE Universe
(
    id INTEGER PRIMARY KEY,
    Name TEXT DEFAULT 'New Universe',
    SortIndex INTEGER DEFAULT 0,
    DefaultTemplateId INTEGER DEFAULT 0
);

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

CREATE TABLE Category
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Name TEXT DEFAULT 'New Category',
    SortIndex INTEGER DEFAULT 0,
    NoteId INTEGER DEFAULT NULL,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(NoteId) REFERENCES Document(id)
);

-- Delete note Document after its Category is deleted.
CREATE TRIGGER Category_NoteDoc_ad AFTER DELETE ON Category BEGIN
  DELETE FROM Document WHERE id = (old.NoteId);
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
    NoteId INTEGER DEFAULT NULL,
    CopyrightPageId INTEGER DEFAULT NULL,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(CategoryId) REFERENCES Category(id) ON DELETE SET NULL,
    FOREIGN KEY(NoteId) REFERENCES Document(id),
    FOREIGN KEY(CopyrightPageId) REFERENCES Document(id)
);

-- Delete note Document after its Story is deleted.
CREATE TRIGGER Story_NoteDoc_ad AFTER DELETE ON Story BEGIN
  DELETE FROM Document WHERE id = (old.NoteId);
  DELETE FROM Document WHERE id = (old.CopyrightPageId);
END;

CREATE TABLE Chapter
(
    id INTEGER PRIMARY KEY,
    StoryId INTEGER,
    Name TEXT DEFAULT 'New Chapter',
    SortIndex INTEGER DEFAULT 0,
    NoteId INTEGER DEFAULT NULL,

    FOREIGN KEY(StoryId) REFERENCES Story(id) ON DELETE CASCADE,
    FOREIGN KEY(NoteId) REFERENCES Document(id)
);

-- Delete note Document after its chapter is deleted.
CREATE TRIGGER Chapter_NoteDoc_ad AFTER DELETE ON Chapter BEGIN
  DELETE FROM Document WHERE id = (old.NoteId);
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
    DocumentId INTEGER DEFAULT NULL,
    NoteId INTEGER DEFAULT NULL,

    FOREIGN KEY(ChapterId) REFERENCES Chapter(id) ON DELETE CASCADE,
    FOREIGN KEY(DocumentId) REFERENCES Document(id),
    FOREIGN KEY(NoteId) REFERENCES Document(id)
);

-- Delete Document after its scene is deleted.
CREATE TRIGGER Scene_Doc_ad AFTER DELETE ON Scene BEGIN
  DELETE FROM Document WHERE id = (old.DocumentId);
  DELETE FROM Document WHERE id = (old.NoteId);
END;

CREATE TABLE Ticket
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Priority INTEGER DEFAULT 2,
    Name TEXT DEFAULT 'New Ticket',
    Status TEXT DEFAULT 'Not Started',
    DueDate TEXT DEFAULT '',
    DocumentId INTEGER DEFAULT NULL,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(DocumentId) REFERENCES Document(id)
);

-- Delete note Document after its ticket is deleted.
CREATE TRIGGER Ticket_NoteDoc_ad AFTER DELETE ON Ticket BEGIN
  DELETE FROM Document WHERE id = (old.DocumentId);
END;

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


-- Full-Text Search Stuff
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


CREATE VIRTUAL TABLE Document_fts USING fts5(PlainText, Content=Document, content_rowid=id);
-- Table rebuild, just here for reference
-- INSERT INTO Document_fts(Document_fts) VALUES ('rebuild');
-- Triggers to keep Document_fts up-to-date
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

CREATE VIRTUAL TABLE NoteDocument_fts USING fts5(Name, Content=NoteDocument, content_rowid=id);
-- Table rebuild, just here for reference
-- INSERT INTO NoteDocument_fts(NoteDocument_fts) VALUES ('rebuild');
-- Triggers to keep NoteDocument_fts up-to-date
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
