using System;

namespace TjMott.Writer.Model.Scripts
{
    public static class DbInitScript
    {
        private static string _init = @"

CREATE TABLE Metadata
(
    id INTEGER PRIMARY KEY,
    Key TEXT UNIQUE,
    Value TEXT
);

INSERT INTO Metadata(Key, Value) VALUES ('DbVersion', 1);
INSERT INTO Metadata(Key, Value) VALUES ('DefaultUniverse', 0);

CREATE TABLE Universe
(
    id INTEGER PRIMARY KEY,
    Name TEXT,
    SortIndex INTEGER,
    MarkdownCss TEXT,
    DefaultTemplateId INTEGER
);

CREATE TABLE FlowDocument
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Xml TEXT,
    PlainText TEXT,
    WordCount INTEGER,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE MarkdownDocument
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    MarkdownText TEXT,
    PlainText TEXT,
    Name TEXT,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE MarkdownCategory
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    ParentId INTEGER,
    Name TEXT,

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
    Name TEXT,
    SortIndex INTEGER,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE Story
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    CategoryId INTEGER,
    Name TEXT,
    Subtitle TEXT,
    Author TEXT,
    Edition TEXT,
    ISBN TEXT,
    ASIN TEXT,
    SortIndex INTEGER,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(CategoryId) REFERENCES Category(id) ON DELETE SET NULL
);

CREATE TABLE File
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Name TEXT,
    FileName TEXT,
    FileType TEXT,
    Data BLOB,
    SortIndex INTEGER,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE Chapter
(
    id INTEGER PRIMARY KEY,
    StoryId INTEGER,
    Name TEXT,
    SortIndex INTEGER,

    FOREIGN KEY(StoryId) REFERENCES Story(id) ON DELETE CASCADE
);

CREATE TABLE Scene
(
    id INTEGER PRIMARY KEY,
    ChapterId INTEGER,
    Name TEXT,
    SortIndex INTEGER,
    ColorA INTEGER,
    ColorR INTEGER,
    ColorG INTEGER,
    ColorB INTEGER,
    FlowDocumentId INTEGER,

    FOREIGN KEY(ChapterId) REFERENCES Chapter(id) ON DELETE CASCADE,
    FOREIGN KEY(FlowDocumentId) REFERENCES FlowDocument(id)
);

-- Delete FlowDocument after its scene is deleted.
CREATE TRIGGER Scene_FlowDoc_ad AFTER DELETE ON Scene BEGIN
  DELETE FROM FlowDocument WHERE id = (old.FlowDocumentId);
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
  INSERT INTO SceneChapter_fts_fts(Chapter_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
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
  INSERT INTO SceneStory_fts_fts(Story_fts, rowid, Name) VALUES ('delete', old.id, old.Name);
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
