using Avalonia.Controls;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Xml;

namespace TjMott.Writer.Models.SqlScripts
{
    public class Update3to4 : DbUpgrader
    {
        private static string _script3to4 = @"
    DROP TABLE FlowDocument;
    DROP TABLE Scene_Old;
    DROP TABLE Chapter_Old;
    DROP TABLE Story_Old;
    DROP TABLE Category_Old;
    DROP TABLE MarkdownCategoryDocument;
    DROP TABLE MarkdownCategory;
    DROP TABLE MarkdownDocument;

    UPDATE Metadata SET Value = 4 WHERE Key = 'DbVersion';
";

        public Update3to4()
        {
            StartVersion = 3;
            TargetVersion = 4;
        }

        public override async Task<bool> DoUpgradeAsync(SqliteConnection connection, Window dialogOwner)
        {
            bool result = await convertCategories(connection, dialogOwner);
            result = await convertStories(connection, dialogOwner);
            result = await convertChapters(connection, dialogOwner);
            result = await convertScenes(connection, dialogOwner);

            // Execute script for v3 to v4 to eliminate MarkdownDocument/FlowDocument.
            result = await runScriptWithVersionCheckAsync(connection, _script3to4, 3, 4);
            return result;
        }

        private async Task<bool> convertCategories(SqliteConnection connection, Window dialogOwner)
        {
            DbCommandHelper cmdGetCategories = new DbCommandHelper(connection);
            cmdGetCategories.Command.CommandText = "SELECT id, UniverseId, Name, SortIndex, MarkdownDocumentId FROM Category_Old";

            DbCommandHelper cmdInsertCategory = new DbCommandHelper(connection);
            cmdInsertCategory.Command.CommandText = "INSERT INTO CATEGORY (id, UniverseId, Name, SortIndex, NoteId) VALUES (@id, @UniverseId, @Name, @SortIndex, @NoteId)";
            cmdInsertCategory.AddParameter("@id");
            cmdInsertCategory.AddParameter("@UniverseId");
            cmdInsertCategory.AddParameter("@Name");
            cmdInsertCategory.AddParameter("@SortIndex");
            cmdInsertCategory.AddParameter("@NoteId");

            using (SqliteDataReader categoryReader = await cmdGetCategories.Command.ExecuteReaderAsync())
            {
                while (await categoryReader.ReadAsync())
                {
                    long categoryId = categoryReader.GetInt64(0);
                    long universeId = categoryReader.GetInt64(1);
                    string name = categoryReader.GetString(2);
                    long sortIndex = categoryReader.GetInt64(3);
                    object mdDocIdObj = categoryReader.GetValue(4);

                    cmdInsertCategory.Parameters["@id"].Value = categoryId;
                    cmdInsertCategory.Parameters["@UniverseId"].Value = universeId;
                    cmdInsertCategory.Parameters["@Name"].Value = name;
                    cmdInsertCategory.Parameters["@SortIndex"].Value = sortIndex;
                    cmdInsertCategory.Parameters["@NoteId"].Value = DBNull.Value;

                    if (mdDocIdObj != DBNull.Value)
                    {
                        // TODO: Migrate MarkdownDocument.
                    }

                    int insertResult = await cmdInsertCategory.Command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        private async Task<bool> convertStories(SqliteConnection connection, Window dialogOwner)
        {
            DbCommandHelper cmdGetStories = new DbCommandHelper(connection);
            cmdGetStories.Command.CommandText = "SELECT id, UniverseId, CategoryId, Name, Subtitle, Author, Edition, ISBN, ASIN, SortIndex, MarkdownDocumentId, FlowDocumentId FROM Story_Old";

            DbCommandHelper cmdInsertStory = new DbCommandHelper(connection);
            cmdInsertStory.SetParameterizedQuery("INSERT INTO Story (id, UniverseId, CategoryId, Name, Subtitle, Author, Edition, ISBN, ASIN, SortIndex, NoteId, CopyrightPageId) VALUES (@id, @UniverseId, @CategoryId, @Name, @Subtitle, @Author, @Edition, @ISBN, @ASIN, @SortIndex, @NoteId, @CopyrightPageId)");
            
            using (SqliteDataReader storyReader = await cmdGetStories.Command.ExecuteReaderAsync())
            {
                while (await storyReader.ReadAsync())
                {
                    long storyId = storyReader.GetInt64(0);
                    long universeId = storyReader.GetInt64(1);
                    object categoryIdObj = storyReader.GetValue(2);
                    string name = storyReader.GetString(3);
                    string subtitle = storyReader.GetString(4);
                    string author = storyReader.GetString(5);
                    string edition = storyReader.GetString(6);
                    string isbn = storyReader.GetString(7);
                    string asin = storyReader.GetString(8);
                    long sortIndex = storyReader.GetInt64(9);
                    object mdDocId = storyReader.GetValue(10);
                    object flowDocId = storyReader.GetValue(11);

                    cmdInsertStory.Parameters["@id"].Value = storyId;
                    cmdInsertStory.Parameters["@UniverseId"].Value = universeId;
                    cmdInsertStory.Parameters["@CategoryId"].Value = DBNull.Value;
                    cmdInsertStory.Parameters["@Name"].Value = name;
                    cmdInsertStory.Parameters["@Subtitle"].Value = subtitle;
                    cmdInsertStory.Parameters["@Author"].Value = author;
                    cmdInsertStory.Parameters["@Edition"].Value = edition;
                    cmdInsertStory.Parameters["@ISBN"].Value = isbn;
                    cmdInsertStory.Parameters["@ASIN"].Value = asin;
                    cmdInsertStory.Parameters["@SortIndex"].Value = sortIndex;
                    cmdInsertStory.Parameters["@NoteId"].Value = DBNull.Value;
                    cmdInsertStory.Parameters["@CopyrightPageId"].Value = DBNull.Value;

                    if (categoryIdObj != DBNull.Value)
                        cmdInsertStory.Parameters["@CategoryId"].Value = (Int64)categoryIdObj;
                    if (mdDocId != DBNull.Value)
                    {
                        // TODO: Migrate MarkdownDocument.
                    }
                    if (flowDocId != DBNull.Value)
                    {
                        cmdInsertStory.Parameters["@CopyrightPageId"].Value = await migrateFlowDocument(connection, (Int64)flowDocId);
                    }
                    int insertResult = await cmdInsertStory.Command.ExecuteNonQueryAsync();
                }
            }
            
            return true;
        }

        private async Task<bool> convertChapters(SqliteConnection connection, Window dialogOwner)
        {
            DbCommandHelper cmdGetChapters = new DbCommandHelper(connection);
            cmdGetChapters.Command.CommandText = "SELECT id, StoryId, Name, SortIndex, MarkdownDocumentId FROM Chapter_Old";

            DbCommandHelper cmdInsertChapter = new DbCommandHelper(connection);
            cmdInsertChapter.SetParameterizedQuery("INSERT INTO Chapter (id, StoryId, Name, SortIndex, NoteId) VALUES (@id, @StoryId, @Name, @SortIndex, @NoteId)");

            using (SqliteDataReader chapterReader  = await cmdGetChapters.Command.ExecuteReaderAsync())
            {
                while (await chapterReader.ReadAsync())
                {
                    long chapterId = chapterReader.GetInt64(0);
                    long storyId = chapterReader.GetInt64(1);
                    string name = chapterReader.GetString(2);
                    long sortIndex = chapterReader.GetInt64(3);
                    object mdDocIdObj = chapterReader.GetValue(4);

                    cmdInsertChapter.Parameters["@id"].Value = chapterId;
                    cmdInsertChapter.Parameters["@StoryId"].Value = storyId;
                    cmdInsertChapter.Parameters["@Name"].Value = name;
                    cmdInsertChapter.Parameters["@SortIndex"].Value = sortIndex;
                    cmdInsertChapter.Parameters["@NoteId"].Value = DBNull.Value;

                    if (mdDocIdObj != DBNull.Value)
                    {
                        // TODO: Migrate MarkdownDocument.
                    }

                    int insertResult = await cmdInsertChapter.Command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        private async Task<bool> convertScenes(SqliteConnection connection, Window dialogOwner)
        {
            // Query all scenes, and convert FlowDocuments to Documents.
            DbCommandHelper cmdGetScenes = new DbCommandHelper(connection);
            cmdGetScenes.Command.CommandText = "SELECT id, ChapterId, Name, SortIndex, ColorA, ColorR, ColorG, ColorB, FlowDocumentId, MarkdownDocumentId FROM Scene_Old";

            DbCommandHelper cmdUpdateScene = new DbCommandHelper(connection);
            cmdUpdateScene.SetParameterizedQuery("INSERT INTO Scene(id, ChapterId, Name, SortIndex, ColorA, ColorR, ColorG, ColorB, DocumentId) VALUES (@id, @ChapterId, @Name, @SortIndex, @ColorA, @ColorR, @ColorG, @ColorB, @DocumentId)");

            
            

            using (SqliteDataReader sceneReader = await cmdGetScenes.Command.ExecuteReaderAsync())
            {
                while (await sceneReader.ReadAsync())
                {
                    long sceneId = sceneReader.GetInt64(0);
                    long chapterId = sceneReader.GetInt64(1);
                    string name = sceneReader.GetString(2);
                    long sortIndex = sceneReader.GetInt64(3);
                    int colorA = sceneReader.GetInt32(4);
                    int colorR = sceneReader.GetInt32(5);
                    int colorG = sceneReader.GetInt32(6);
                    int colorB = sceneReader.GetInt32(7);
                    long flowDocId = sceneReader.GetInt64(8);
                    object mdDocIdObj = sceneReader.GetValue(9);

                    if (mdDocIdObj != DBNull.Value)
                    {
                        // TODO: Migrate markdown document.
                    }

                    // Get FlowDocument for this scene.
                    long docId = await migrateFlowDocument(connection, flowDocId);

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
                    int insertResult = await cmdUpdateScene.Command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        private async Task<long> migrateFlowDocument(SqliteConnection connection, long flowDocId)
        {
            DbCommandHelper cmdGetFlowDocument = new DbCommandHelper(connection);
            cmdGetFlowDocument.SetParameterizedQuery("SELECT id, UniverseId, Xml, PlainText, WordCount, IsEncrypted FROM FlowDocument WHERE id = @id");

            DbCommandHelper cmdInsertDocument = new DbCommandHelper(connection);
            cmdInsertDocument.SetParameterizedQuery("INSERT INTO Document(UniverseId, Json, PlainText, WordCount, IsEncrypted) VALUES (@universeId, @json, @plainText, @wordCount, @isEncrypted)");
            
            SqliteCommand cmdGetId = new SqliteCommand("select last_insert_rowid()", connection);

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
                if (insertResult != 1)
                    return -1;
                // Get ID of new Document.
                long docId = (long)await cmdGetId.ExecuteScalarAsync();
                return docId;
            }
        }

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
                    // Ignore defaults
                    if (font != "Garamond")
                        inheritAttributes["font"] = font;
                }
                if (xml.DocumentElement.HasAttribute("FontSize"))
                {
                    string size = xml.DocumentElement.GetAttribute("FontSize");
                    // Ignore defaults
                    if (size != "12")
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
