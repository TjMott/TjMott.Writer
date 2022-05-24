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

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE Category
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Name TEXT DEFAULT 'New Category',
    SortIndex INTEGER DEFAULT 0,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);


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

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE,
    FOREIGN KEY(CategoryId) REFERENCES Category(id) ON DELETE SET NULL
);


CREATE TABLE File
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Name TEXT DEFAULT 'New File',
    FileName TEXT DEFAULT '',
    FileType TEXT DEFAULT '',
    Data BLOB,
    SortIndex INTEGER DEFAULT 0,

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

CREATE TABLE Chapter
(
    id INTEGER PRIMARY KEY,
    StoryId INTEGER,
    Name TEXT DEFAULT 'New Chapter',
    SortIndex INTEGER DEFAULT 0,

    FOREIGN KEY(StoryId) REFERENCES Story(id) ON DELETE CASCADE
);

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

    FOREIGN KEY(ChapterId) REFERENCES Chapter(id) ON DELETE CASCADE,
    FOREIGN KEY(DocumentId) REFERENCES Document(id)
);

-- Delete Document after its scene is deleted.
CREATE TRIGGER Scene_Doc_ad AFTER DELETE ON Scene BEGIN
  DELETE FROM Document WHERE id = (old.DocumentId);
END;

CREATE TABLE Ticket
(
    id INTEGER PRIMARY KEY,
    UniverseId INTEGER,
    Priority INTEGER DEFAULT 2,
    Name TEXT DEFAULT 'New Ticket',
    Status TEXT DEFAULT 'Not Started',
    DueDate TEXT DEFAULT '',

    FOREIGN KEY(UniverseId) REFERENCES Universe(id) ON DELETE CASCADE
);

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
