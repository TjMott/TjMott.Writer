using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TjMott.Writer.Models.SqlScripts
{
    public class Update2to4 : DbUpgrader
    {
        private static string _script2to3 = @"

    -- Remove FlowDocument FTS
    DROP TRIGGER FlowDocument_ai;
    DROP TRIGGER FlowDocument_ad;
    DROP TRIGGER FlowDocument_au;
    DROP TABLE FlowDocument_fts;

    -- Remove MarkdownDocument FTS
    DROP TRIGGER MarkdownDocument_ai;
    DROP TRIGGER MarkdownDocument_ad;
    DROP TRIGGER MarkdownDocument_au;
    DROP TABLE MarkdownDocument_fts;

    -- Add new Document table (which replaces FlowDocument and MarkdownDocument)
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

    -- Create new Scene table
    DROP TRIGGER Scene_ai;
    DROP TRIGGER Scene_au;
    DROP TRIGGER Scene_ad;
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

    --

    UPDATE Metadata SET Value = 3 WHERE Key = 'DbVersion';
";

        private static string _script3to4 = @"

    -- Remove FlowDocument stuff completely.
    DROP TABLE FlowDocument;

    DROP TABLE Scene_Old;

    UPDATE Metadata SET Value = 4 WHERE Key = 'DbVersion';
";

        public Update2to4()
        {
            StartVersion = 2;
            TargetVersion = 4;
        }

        public override async Task<bool> DoUpgradeAsync(SqliteConnection connection, Window dialogOwner)
        {
            bool result = await runScriptWithVersionCheckAsync(connection, _script2to3, 2, 3);
            if (!result)
                return result;

            // Query all scenes, and convert FlowDocuments to Documents.
            DbCommandHelper cmdGetScenes = new DbCommandHelper(connection);
            cmdGetScenes.Command.CommandText = "SELECT id, ChapterId, Name, SortIndex, ColorA, ColorR, ColorG, ColorB, FlowDocumentId, MarkdownDocumentId FROM Scene_Old";

            DbCommandHelper cmdUpdateScene = new DbCommandHelper(connection);
            cmdUpdateScene.Command.CommandText = "INSERT INTO Scene(id, ChapterId, Name, SortIndex, ColorA, ColorR, ColorG, ColorB, DocumentId) VALUES (@id, @ChapterId, @Name, @SortIndex, @ColorA, @ColorR, @ColorG, @ColorB, @DocumentId)";
            cmdUpdateScene.AddParameter("@id");
            cmdUpdateScene.AddParameter("@ChapterId");
            cmdUpdateScene.AddParameter("@Name");
            cmdUpdateScene.AddParameter("@SortIndex");
            cmdUpdateScene.AddParameter("@ColorA");
            cmdUpdateScene.AddParameter("@ColorR");
            cmdUpdateScene.AddParameter("@ColorG");
            cmdUpdateScene.AddParameter("@ColorB");
            cmdUpdateScene.AddParameter("@DocumentId");

            DbCommandHelper cmdGetFlowDocument = new DbCommandHelper(connection);
            cmdGetFlowDocument.Command.CommandText = "SELECT id, UniverseId, Xml, PlainText, WordCount, IsEncrypted FROM FlowDocument WHERE id = @id";
            cmdGetFlowDocument.AddParameter("@id");

            DbCommandHelper cmdInsertDocument = new DbCommandHelper(connection);
            cmdInsertDocument.Command.CommandText = "INSERT INTO Document(UniverseId, Json, PlainText, WordCount, IsEncrypted) VALUES (@universeId, @json, @plainText, @wordCount, @isEncrypted)";
            cmdInsertDocument.AddParameter("@universeId");
            cmdInsertDocument.AddParameter("@json");
            cmdInsertDocument.AddParameter("@plainText");
            cmdInsertDocument.AddParameter("@wordCount");
            cmdInsertDocument.AddParameter("@isEncrypted");

            SqliteCommand cmdGetId = new SqliteCommand("select last_insert_rowid()", connection);

            using (SqliteDataReader sceneReader = await cmdGetScenes.Command.ExecuteReaderAsync())
            {
                while (await sceneReader.ReadAsync())
                {
                    long sceneId = sceneReader.GetInt64(0);
                    long chapterId = sceneReader.GetInt64(1);
                    string name = sceneReader.GetString(2);
                    long sortIndex =sceneReader.GetInt64(3);
                    int colorA = sceneReader.GetInt32(4);
                    int colorR = sceneReader.GetInt32(5);
                    int colorG = sceneReader.GetInt32(6);
                    int colorB = sceneReader.GetInt32(7);
                    long flowDocId = sceneReader.GetInt64(8);
                    object markdownDocIdObj = sceneReader.GetValue(9);

                    // Get FlowDocument for this scene.
                    cmdGetFlowDocument.Parameters["@id"].Value = flowDocId;
                    using (SqliteDataReader flowDocReader = await cmdGetFlowDocument.Command.ExecuteReaderAsync())
                    {
                        await flowDocReader.ReadAsync();
                        long universeId = flowDocReader.GetInt64(1);
                        string xml = flowDocReader.GetString(2);
                        string plainText = flowDocReader.GetString(3);
                        long wordCount = flowDocReader.GetInt64(4);
                        bool isEncrypted = flowDocReader.GetBoolean(5);
                        string json = "";

                        if (isEncrypted)
                        {
                            // TODO
                        }
                        else
                        {
                            // Convert FlowDocument XML to QuillJS JSON.
                            json = await Task.Run(() => FlowDocToJsonConverter.Convert(xml));
                        }

                        // Insert Document into database.
                        cmdInsertDocument.Parameters["@universeId"].Value = universeId;
                        cmdInsertDocument.Parameters["@json"].Value = json;
                        cmdInsertDocument.Parameters["@plainText"].Value = plainText;
                        cmdInsertDocument.Parameters["@wordCount"].Value = wordCount;
                        cmdInsertDocument.Parameters["@isEncrypted"].Value = isEncrypted;
                        int insertResult = await cmdInsertDocument.Command.ExecuteNonQueryAsync();

                        // Get ID of new Document.
                        long docId = (long)await cmdGetId.ExecuteScalarAsync();

                        // Copy data over to new Scene table.
                        cmdUpdateScene.Parameters["@id"].Value = sceneId;
                        cmdUpdateScene.Parameters["@ChapterId"].Value = chapterId;
                        cmdUpdateScene.Parameters["@Name"].Value = name;
                        cmdUpdateScene.Parameters["@SortIndex"].Value = sortIndex;
                        cmdUpdateScene.Parameters["@ColorA"].Value = colorA;
                        cmdUpdateScene.Parameters["@ColorR"].Value = colorR;
                        cmdUpdateScene.Parameters["@ColorG"].Value = colorG;
                        cmdUpdateScene.Parameters["@ColorB"].Value = colorB;
                        cmdUpdateScene.Parameters["@DocumentId"].Value = docId;
                        insertResult = await cmdUpdateScene.Command.ExecuteNonQueryAsync();
                    }
                }
            }

            // TODO: Update MarkdownDocuments.

            // Execute script for v3 to v4 to eliminate MarkdownDocument/FlowDocument.
            result = await runScriptWithVersionCheckAsync(connection, _script3to4, 3, 4);

            return result;
        } // DoUpgradeAsync

        private static class FlowDocToJsonConverter
        {
            public static string Convert(string xmlString)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlString);
                if (xml.DocumentElement.Name != "FlowDocument") throw new ApplicationException("Expected FlowDocument!");

                dynamic json = new ExpandoObject();
                json.ops = new List<dynamic>();

                Dictionary<string, object> inheritAttributes = new Dictionary<string, object>();

                if (xml.DocumentElement.HasAttribute("FontFamily"))
                {
                    string font = xml.DocumentElement.GetAttribute("FontFamily");
                    inheritAttributes["font"] = font;
                }
                if (xml.DocumentElement.HasAttribute("FontSize"))
                {
                    string size = xml.DocumentElement.GetAttribute("FontSize");
                    inheritAttributes["size"] = size + "px";
                }

                foreach (XmlElement node in xml.DocumentElement.ChildNodes)
                {
                    processElement(node, json, inheritAttributes);
                }

                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
                return jsonString;
            }

            static void processElement(XmlNode e, dynamic json, Dictionary<string, object> inheritAttributes)
            {
                Dictionary<string, object> subatts = new Dictionary<string, object>(inheritAttributes);
                if (e is XmlText)
                {
                    processText(e as XmlText, json, subatts);
                }
                else if (e.Name == "Paragraph")
                {
                    processParagraph(e as XmlElement, json, subatts);
                }
                else if (e.Name == "Run")
                {
                    processRun(e as XmlElement, json, subatts);
                }
                else if (e.Name == "List")
                {
                    processList(e as XmlElement, json, subatts);
                }
                else if (e.Name == "ListItem")
                {
                    processListItem(e as XmlElement, json, subatts);
                }
            }
            static void processParagraph(XmlElement p, dynamic json, Dictionary<string, object> inheritAttributes)
            {
                Dictionary<string, object> subatts = new Dictionary<string, object>(inheritAttributes);
                if (p.HasAttribute("TextAlignment"))
                {

                }
                foreach (XmlNode node in p.ChildNodes)
                {
                    processElement(node, json, subatts);
                }
                // Add newline after paragraph.
                dynamic newline = new ExpandoObject();
                newline.insert = "\n";
                if (subatts.Count > 0)
                    newline.attributes = subatts;
                json.ops.Add(newline);
            }

            static void processText(XmlText t, dynamic json, Dictionary<string, object> inheritAttributes)
            {
                // Process text. Split on newlines and turn each line into its own insert.
                string[] text = t.InnerText.Split('\n');
                for (int i = 0; i < text.Length; i++)
                {
                    dynamic run = new ExpandoObject();
                    if (inheritAttributes.Count > 0)
                        run.attributes = inheritAttributes;
                    run.insert = text[i];
                    if (i < text.Length - 1)
                        run.insert += "\n";
                    json.ops.Add(run);
                }
            }

            static void processRun(XmlElement r, dynamic json, Dictionary<string, object> inheritAttributes)
            {
                Dictionary<string, object> attributes = new Dictionary<string, object>(inheritAttributes);
                if (r.HasAttribute("FontStyle"))
                {
                    string fontStyle = r.GetAttribute("FontStyle");
                    if (fontStyle == "Italic")
                    {
                        attributes["italic"] = true;
                    }
                }
                if (r.HasAttribute("FontSize"))
                {
                    string size = r.GetAttribute("FontSize");
                    attributes["size"] = size + "px";
                }
                if (r.HasAttribute("Foreground"))
                {
                    string textColor = r.GetAttribute("Foreground");
                    // This color was default text color in the old WPF editor, so ignore it.
                    if (textColor != "#FF00000A")
                    {
                        // Chop off alpha channel.
                        textColor = "#" + textColor.Substring(3);
                        attributes["color"] = textColor;
                    }
                }
                if (r.HasAttribute("FontWeight"))
                {
                    string weight = r.GetAttribute("FontWeight");
                    if (weight == "Bold")
                    {
                        attributes["bold"] = true;
                    }
                }
                if (r.HasAttribute("FontFamily"))
                {
                    string font = r.GetAttribute("FontFamily");
                    attributes["font"] = font;
                }

                // Process text. Split on newlines and turn each line into its own insert.
                string[] text = r.InnerText.Split('\n');
                for (int i = 0; i < text.Length; i++)
                {
                    dynamic run = new ExpandoObject();
                    if (attributes.Count > 0)
                        run.attributes = attributes;
                    run.insert = text[i];
                    if (i < text.Length - 1)
                        run.insert += "\n";
                    json.ops.Add(run);
                }
            }

            static void processList(XmlElement l, dynamic json, Dictionary<string, object> inheritAttributes)
            {
                Dictionary<string, object> subatts = new Dictionary<string, object>(inheritAttributes);
                foreach (XmlElement node in l.ChildNodes)
                {
                    processElement(node, json, subatts);
                }
            }

            static void processListItem(XmlElement l, dynamic json, Dictionary<string, object> inheritAttributes)
            {
                Dictionary<string, object> subatts = new Dictionary<string, object>(inheritAttributes);
                foreach (XmlElement node in l.ChildNodes)
                {
                    processElement(node, json, subatts);
                }
            }
        }
    }
}
