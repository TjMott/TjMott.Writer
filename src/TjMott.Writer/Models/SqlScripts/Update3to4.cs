using Avalonia.Controls;
using MessageBox.Avalonia.DTO;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using TjMott.Writer.ViewModels;

namespace TjMott.Writer.Models.SqlScripts
{
    public class Update3to4 : DbUpgrader
    {
        private static List<string> _aesPasswords = new List<string>();

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

        private SqliteConnection _connection;
        private List<long> _migratedMarkdownIds;
        private Window _dialogOwner;
        private SqliteCommand _cmdGetNewId;

        public Update3to4()
        {
            StartVersion = 3;
            TargetVersion = 4;
        }

        public override async Task<bool> DoUpgradeAsync(SqliteConnection connection, Window dialogOwner)
        {
            _connection = connection;
            _dialogOwner = dialogOwner;
            _migratedMarkdownIds = new List<long>();
            _cmdGetNewId = new SqliteCommand("select last_insert_rowid()", _connection);

            bool result = await convertCategories();
            result = await convertStories();
            result = await convertChapters();
            result = await convertScenes();
            result = await convertNotes();

            result = await runScriptWithVersionCheckAsync(_connection, _script3to4, 3, 4);
            return result;
        }

        private async Task<bool> convertCategories()
        {
            DbCommandHelper cmdGetCategories = new DbCommandHelper(_connection);
            cmdGetCategories.Command.CommandText = "SELECT id, UniverseId, Name, SortIndex, MarkdownDocumentId FROM Category_Old";

            DbCommandHelper cmdInsertCategory = new DbCommandHelper(_connection);
            cmdInsertCategory.Command.CommandText = "INSERT INTO CATEGORY (id, UniverseId, Name, SortIndex) VALUES (@id, @UniverseId, @Name, @SortIndex)";
            cmdInsertCategory.AddParameter("@id");
            cmdInsertCategory.AddParameter("@UniverseId");
            cmdInsertCategory.AddParameter("@Name");
            cmdInsertCategory.AddParameter("@SortIndex");

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

                    if (mdDocIdObj != DBNull.Value)
                    {
                        // Migrate MarkdownDocument.
                        long mdDocId = (long)mdDocIdObj;
                        long newDocId = await migrateMarkdownDocument(mdDocId, "Note");
                        _migratedMarkdownIds.Add(mdDocId);

                        // Categorize.
                        await categorizeAttachedDoc(universeId, newDocId, string.Format("Category Note: {0}", name));
                    }

                    int insertResult = await cmdInsertCategory.Command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        private async Task<bool> convertStories()
        {
            DbCommandHelper cmdGetStories = new DbCommandHelper(_connection);
            cmdGetStories.Command.CommandText = "SELECT id, UniverseId, CategoryId, Name, Subtitle, Author, Edition, ISBN, ASIN, SortIndex, MarkdownDocumentId, FlowDocumentId FROM Story_Old";

            DbCommandHelper cmdInsertStory = new DbCommandHelper(_connection);
            cmdInsertStory.SetParameterizedQuery("INSERT INTO Story (id, UniverseId, CategoryId, Name, Subtitle, Author, Edition, ISBN, ASIN, SortIndex, CopyrightPageId) VALUES (@id, @UniverseId, @CategoryId, @Name, @Subtitle, @Author, @Edition, @ISBN, @ASIN, @SortIndex, @CopyrightPageId)");
            
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
                    object mdDocIdObj = storyReader.GetValue(10);
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
                    cmdInsertStory.Parameters["@CopyrightPageId"].Value = DBNull.Value;

                    if (categoryIdObj != DBNull.Value)
                        cmdInsertStory.Parameters["@CategoryId"].Value = (Int64)categoryIdObj;
                    if (mdDocIdObj != DBNull.Value)
                    {
                        // Migrate MarkdownDocument.
                        long mdDocId = (long)mdDocIdObj;
                        long newDocId = await migrateMarkdownDocument(mdDocId, "Note");
                        _migratedMarkdownIds.Add(mdDocId);

                        // Categorize.
                        await categorizeAttachedDoc(universeId, newDocId, string.Format("Story Note: {0}", name));
                    }
                    if (flowDocId != DBNull.Value)
                    {
                        cmdInsertStory.Parameters["@CopyrightPageId"].Value = await migrateFlowDocument((Int64)flowDocId);
                    }
                    int insertResult = await cmdInsertStory.Command.ExecuteNonQueryAsync();
                }
            }
            
            return true;
        }

        private async Task<bool> convertChapters()
        {
            DbCommandHelper cmdGetChapters = new DbCommandHelper(_connection);
            cmdGetChapters.Command.CommandText = "SELECT Chapter_Old.id, Chapter_Old.StoryId, Chapter_Old.Name, Chapter_Old.SortIndex, Chapter_Old.MarkdownDocumentId, Story_Old.UniverseId FROM Chapter_Old, Story_Old WHERE Chapter_Old.StoryId = Story_Old.id";

            DbCommandHelper cmdInsertChapter = new DbCommandHelper(_connection);
            cmdInsertChapter.SetParameterizedQuery("INSERT INTO Chapter (id, StoryId, Name, SortIndex) VALUES (@id, @StoryId, @Name, @SortIndex)");

            using (SqliteDataReader chapterReader  = await cmdGetChapters.Command.ExecuteReaderAsync())
            {
                while (await chapterReader.ReadAsync())
                {
                    long chapterId = chapterReader.GetInt64(0);
                    long storyId = chapterReader.GetInt64(1);
                    string name = chapterReader.GetString(2);
                    long sortIndex = chapterReader.GetInt64(3);
                    object mdDocIdObj = chapterReader.GetValue(4);
                    long universeId = chapterReader.GetInt64(5);

                    cmdInsertChapter.Parameters["@id"].Value = chapterId;
                    cmdInsertChapter.Parameters["@StoryId"].Value = storyId;
                    cmdInsertChapter.Parameters["@Name"].Value = name;
                    cmdInsertChapter.Parameters["@SortIndex"].Value = sortIndex;

                    if (mdDocIdObj != DBNull.Value)
                    {
                        // Migrate MarkdownDocument.
                        long mdDocId = (long)mdDocIdObj;
                        long newDocId = await migrateMarkdownDocument(mdDocId, "Note");
                        _migratedMarkdownIds.Add(mdDocId);

                        // Categorize.
                        await categorizeAttachedDoc(universeId, newDocId, string.Format("Chapter Note: {0}", name));
                    }

                    int insertResult = await cmdInsertChapter.Command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        private async Task<bool> convertScenes()
        {
            // Query all scenes, and convert FlowDocuments to Documents.
            DbCommandHelper cmdGetScenes = new DbCommandHelper(_connection);
            cmdGetScenes.Command.CommandText = "SELECT Scene_Old.id, Scene_Old.ChapterId, Scene_Old.Name, Scene_Old.SortIndex, Scene_Old.ColorA, Scene_Old.ColorR, Scene_Old.ColorG, Scene_Old.ColorB, Scene_Old.FlowDocumentId, Scene_Old.MarkdownDocumentId, Story_Old.UniverseId FROM Scene_Old, Chapter_Old, Story_Old WHERE Scene_Old.ChapterId = Chapter_Old.id AND Chapter_Old.StoryId = Story_Old.id";

            DbCommandHelper cmdInsertScene = new DbCommandHelper(_connection);
            cmdInsertScene.SetParameterizedQuery("INSERT INTO Scene(id, ChapterId, Name, SortIndex, ColorA, ColorR, ColorG, ColorB, DocumentId) VALUES (@id, @ChapterId, @Name, @SortIndex, @ColorA, @ColorR, @ColorG, @ColorB, @DocumentId)");

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
                    long universeId = sceneReader.GetInt64(10);

                    // Copy data over to new Scene table.
                    cmdInsertScene.Parameters["@id"].Value = sceneId;
                    cmdInsertScene.Parameters["@ChapterId"].Value = chapterId;
                    cmdInsertScene.Parameters["@Name"].Value = name;
                    cmdInsertScene.Parameters["@SortIndex"].Value = sortIndex;
                    cmdInsertScene.Parameters["@ColorA"].Value = colorA;
                    cmdInsertScene.Parameters["@ColorR"].Value = colorR;
                    cmdInsertScene.Parameters["@ColorG"].Value = colorG;
                    cmdInsertScene.Parameters["@ColorB"].Value = colorB;
                    
                    if (mdDocIdObj != DBNull.Value)
                    {
                        // Migrate MarkdownDocument.
                        long mdDocId = (long)mdDocIdObj;
                        long newDocId = await migrateMarkdownDocument(mdDocId, "Note");
                        _migratedMarkdownIds.Add(mdDocId);

                        // Categorize.
                        await categorizeAttachedDoc(universeId, newDocId, string.Format("Scene Note: {0}", name));
                    }

                    // Get FlowDocument for this scene.
                    long docId = await migrateFlowDocument(flowDocId);
                    cmdInsertScene.Parameters["@DocumentId"].Value = docId;

                    int insertResult = await cmdInsertScene.Command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        private async Task categorizeAttachedDoc(long universeId, long docId, string docName)
        {
            const string categoryName = "Imported Attached Notes";
            DbCommandHelper cmdGetCategory = new DbCommandHelper(_connection);
            cmdGetCategory.SetParameterizedQuery("SELECT id FROM NoteCategory WHERE UniverseId = @UniverseId AND Name = @CategoryName");
            cmdGetCategory.Parameters["@UniverseId"].Value = universeId;
            cmdGetCategory.Parameters["@CategoryName"].Value = categoryName;
            long catId = -1;

            using (SqliteDataReader catReader = await cmdGetCategory.Command.ExecuteReaderAsync())
            {
                if (await catReader.ReadAsync())
                {
                    catId = catReader.GetInt64(0);
                }
            }

            if (catId == -1)
            {
                // Doesn't exist, create it.
                DbCommandHelper cmdCreateCategory = new DbCommandHelper(_connection);
                cmdCreateCategory.SetParameterizedQuery("INSERT INTO NoteCategory(UniverseId, Name) VALUES (@UniverseId, @Name)");
                cmdCreateCategory.Parameters["@UniverseId"].Value = universeId;
                cmdCreateCategory.Parameters["@Name"].Value = categoryName;
                int insertResult = await cmdCreateCategory.Command.ExecuteNonQueryAsync();
                catId = (long)await _cmdGetNewId.ExecuteScalarAsync();
            }


            // Create NoteDocument.
            DbCommandHelper cmdCreateNoteDoc = new DbCommandHelper(_connection);
            cmdCreateNoteDoc.SetParameterizedQuery("INSERT INTO NoteDocument(UniverseId, DocumentId, Name) VALUES (@UniverseId, @DocumentId, @Name)");
            cmdCreateNoteDoc.Parameters["@UniverseId"].Value = universeId;
            cmdCreateNoteDoc.Parameters["@DocumentId"].Value = docId;
            cmdCreateNoteDoc.Parameters["@Name"].Value = docName;
            await cmdCreateNoteDoc.Command.ExecuteNonQueryAsync();
            long noteDocId = (long)await _cmdGetNewId.ExecuteScalarAsync();

            // Create NoteCategoryDocument.
            DbCommandHelper cmdCreateNoteCatDoc = new DbCommandHelper(_connection);
            cmdCreateNoteCatDoc.SetParameterizedQuery("INSERT INTO NoteCategoryDocument(NoteDocumentId, NoteCategoryId) VALUES (@NoteDocumentId, @NoteCategoryId)");
            cmdCreateNoteCatDoc.Parameters["@NoteDocumentId"].Value = noteDocId;
            cmdCreateNoteCatDoc.Parameters["@NoteCategoryId"].Value = catId;
            await cmdCreateNoteCatDoc.Command.ExecuteNonQueryAsync();
        }

        private async Task<long> migrateMarkdownDocument(long markdownDocId, string documentType)
        {
            DbCommandHelper cmdGetMarkdownDoc = new DbCommandHelper(_connection);
            cmdGetMarkdownDoc.SetParameterizedQuery("SELECT id, UniverseId, MarkdownText, PlainText, Name FROM MarkdownDocument WHERE id = @id");
            cmdGetMarkdownDoc.Parameters["@id"].Value = markdownDocId;

            DbCommandHelper cmdInsertDoc = new DbCommandHelper(_connection);
            cmdInsertDoc.SetParameterizedQuery("INSERT INTO Document (UniverseId, Json, PlainText, WordCount, IsEncrypted, DocumentType) VALUES (@UniverseId, @Json, @PlainText, @WordCount, @IsEncrypted, @DocumentType)");

            using (SqliteDataReader docReader = await cmdGetMarkdownDoc.Command.ExecuteReaderAsync())
            {
                await docReader.ReadAsync();
                long id = docReader.GetInt64(0);
                long universeId = docReader.GetInt64(1);
                string markdownText = docReader.GetString(2);
                string plainText = docReader.GetString(3);
                string name = docReader.GetString(4);

                // For now, dump the text into a single delta without really converting it.
                dynamic json = new ExpandoObject();
                json.ops = new List<dynamic>();
                dynamic newDoc = new ExpandoObject();
                newDoc.insert = markdownText;
                json.ops.Add(newDoc);

                // Insert the new Document.
                cmdInsertDoc.Parameters["@UniverseId"].Value = universeId;
                cmdInsertDoc.Parameters["@PlainText"].Value = plainText;
                cmdInsertDoc.Parameters["@Json"].Value = Newtonsoft.Json.JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);
                cmdInsertDoc.Parameters["@IsEncrypted"].Value = false;
                cmdInsertDoc.Parameters["@DocumentType"].Value = documentType;
                cmdInsertDoc.Parameters["@WordCount"].Value = markdownText.Split(new char[] { '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
                int insertResult = await cmdInsertDoc.Command.ExecuteNonQueryAsync();
                if (insertResult != 1)
                    return -1;
                // Get ID of new Document.
                long docId = (long)await _cmdGetNewId.ExecuteScalarAsync();
                return docId;
            }
        }

        private async Task<bool> convertNotes()
        {
            // Need to maintain mapping of IDs to properly convert note categories.
            // The IDs change because we are merging both FlowDocuments and MarkdownDocuments into the new Document table.
            // Category IDs etc will not change.
            Dictionary<long, long> markdownToDocIdMapping = new Dictionary<long, long>();
            Dictionary<long, long> categoryOldToNewIdMapping = new Dictionary<long, long>();

            DbCommandHelper cmdGetCategories = new DbCommandHelper(_connection);
            cmdGetCategories.SetParameterizedQuery("SELECT id, UniverseId, ParentId, Name FROM MarkdownCategory");

            DbCommandHelper cmdInsertCategory = new DbCommandHelper(_connection);
            cmdInsertCategory.SetParameterizedQuery("INSERT INTO NoteCategory (UniverseId, ParentId, Name) VALUES (@UniverseId, @ParentId, @Name)");

            // First migrate the note categories.
            using (SqliteDataReader catReader = await cmdGetCategories.Command.ExecuteReaderAsync())
            {
                while (await catReader.ReadAsync())
                {
                    long id = catReader.GetInt64(0);
                    long universeId = catReader.GetInt64(1);
                    object parentIdObj = catReader.GetValue(2);
                    string name = catReader.GetString(3);

                    cmdInsertCategory.Parameters["@UniverseId"].Value = universeId;
                    cmdInsertCategory.Parameters["@ParentId"].Value = DBNull.Value;
                    cmdInsertCategory.Parameters["@Name"].Value = name;

                    if (parentIdObj != DBNull.Value)
                    {
                        cmdInsertCategory.Parameters["@ParentId"].Value = (long)parentIdObj;
                    }

                    int insertResult = await cmdInsertCategory.Command.ExecuteNonQueryAsync();
                    long newId = (long)await _cmdGetNewId.ExecuteScalarAsync();
                    categoryOldToNewIdMapping[id] = newId;
                }
            }

            DbCommandHelper cmdGetDocs = new DbCommandHelper(_connection);
            cmdGetDocs.SetParameterizedQuery("SELECT id, UniverseId, MarkdownText, PlainText, Name, IsSpecial FROM MarkdownDocument");

            DbCommandHelper cmdInsertNoteDoc = new DbCommandHelper(_connection);
            cmdInsertNoteDoc.SetParameterizedQuery("INSERT INTO NoteDocument (UniverseId, DocumentId, Name) VALUES (@UniverseId, @DocumentId, @Name)");

            // Now migrate note documents that were not already migrated as part of scenes/chapters/tickets etc.
            using (SqliteDataReader docReader = await cmdGetDocs.Command.ExecuteReaderAsync())
            {
                while (await docReader.ReadAsync())
                {
                    long id = docReader.GetInt64(0);
                    long universeId = docReader.GetInt64(1);
                    string name = docReader.GetString(4);

                    // Skip if this MarkdownDoc was already converted, which can
                    // happen if it was attached to some other item (chapter, scene, ticket, etc)
                    if (_migratedMarkdownIds.Contains(id))
                        continue;

                    // Convert to Document.
                    long docId = await migrateMarkdownDocument(id, "Note");

                    // Create NoteDocument entry.
                    cmdInsertNoteDoc.Parameters["@UniverseId"].Value = universeId;
                    cmdInsertNoteDoc.Parameters["@DocumentId"].Value = docId;
                    cmdInsertNoteDoc.Parameters["@Name"].Value = name;
                    int insertResult = await cmdInsertNoteDoc.Command.ExecuteNonQueryAsync();

                    // Get new ID, save mapping.
                    long newId = (long)await _cmdGetNewId.ExecuteScalarAsync();
                    markdownToDocIdMapping[id] = newId;

                    // Probably not needed.
                    _migratedMarkdownIds.Add(id);
                }
            }

            // Migrate the category/document mappings.
            DbCommandHelper cmdGetCatDocs = new DbCommandHelper(_connection);
            cmdGetCatDocs.SetParameterizedQuery("SELECT id, MarkdownCategoryId, MarkdownDocumentId FROM MarkdownCategoryDocument");

            DbCommandHelper cmdInsertCatDoc = new DbCommandHelper(_connection);
            cmdInsertCatDoc.SetParameterizedQuery("INSERT INTO NoteCategoryDocument (NoteDocumentId, NoteCategoryId) VALUES (@NoteDocumentId, @NoteCategoryId)");

            using (SqliteDataReader catDocReader = await cmdGetCatDocs.Command.ExecuteReaderAsync())
            {
                while (await catDocReader.ReadAsync())
                {
                    long id = catDocReader.GetInt64(0);
                    long markdownCatId = catDocReader.GetInt64(1);
                    long markdownDocId = catDocReader.GetInt64(2);

                    cmdInsertCatDoc.Parameters["@NoteDocumentId"].Value = markdownToDocIdMapping[markdownDocId];
                    cmdInsertCatDoc.Parameters["@NoteCategoryId"].Value = categoryOldToNewIdMapping[markdownCatId];
                    int insertResult = await cmdInsertCatDoc.Command.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        private async Task<long> migrateFlowDocument(long flowDocId)
        {
            DbCommandHelper cmdGetFlowDocument = new DbCommandHelper(_connection);
            cmdGetFlowDocument.SetParameterizedQuery("SELECT id, UniverseId, Xml, PlainText, WordCount, IsEncrypted FROM FlowDocument WHERE id = @id");

            DbCommandHelper cmdInsertDocument = new DbCommandHelper(_connection);
            cmdInsertDocument.SetParameterizedQuery("INSERT INTO Document(UniverseId, Json, PlainText, WordCount, IsEncrypted, DocumentType) VALUES (@universeId, @json, @plainText, @wordCount, @isEncrypted, @DocumentType)");

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
                string aesKey = "";
                bool decrypted = true;

                if (isEncrypted)
                {
                    decrypted = false;
                    if (_aesPasswords.Count == 0)
                    {
                        MessageBoxInputParams mbp = new MessageBoxInputParams();
                        mbp.IsPassword = true;
                        mbp.ContentTitle = "FlowDocument is encrypted";
                        mbp.ContentMessage = "Enter your AES password.";
                        mbp.Icon = MessageBox.Avalonia.Enums.Icon.Question;
                        mbp.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxInputWindow(mbp);
                        var msgboxResult = await msgbox.ShowDialog(_dialogOwner);
                        if (msgboxResult.Button == "Confirm")
                        {
                            _aesPasswords.Add(msgboxResult.Message);
                        }
                    }
                    for (int i = 0; i < _aesPasswords.Count; i++)
                    {
                        string tmpPass = _aesPasswords[i];
                        try
                        {
                            xml = AESHelper.AesDecrypt(xml, tmpPass);
                            aesKey = tmpPass;
                            decrypted = true;
                            break;
                        }
                        catch (CryptographicException)
                        {
                            if (i == _aesPasswords.Count - 1)
                            {
                                MessageBoxInputParams mbp = new MessageBoxInputParams();
                                mbp.IsPassword = true;
                                mbp.ContentTitle = "FlowDocument is encrypted";
                                mbp.ContentMessage = "Enter your AES password.";
                                mbp.Icon = MessageBox.Avalonia.Enums.Icon.Question;
                                mbp.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                var msgbox = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxInputWindow(mbp);
                                var msgboxResult = await msgbox.ShowDialog(_dialogOwner);
                                if (msgboxResult.Button == "Confirm")
                                {
                                    _aesPasswords.Add(msgboxResult.Message);
                                }
                            }
                        }
                    }
                }
                if (decrypted)
                {
                    // Convert FlowDocument XML to QuillJS JSON.
                    json = await Task.Run(() => FlowDocToJsonConverter.Convert(xml));
                }

                if (isEncrypted)
                {
                    json = AESHelper.AesEncrypt(json, aesKey);
                }

                // Insert Document into database.
                cmdInsertDocument.Parameters["@universeId"].Value = universeId;
                cmdInsertDocument.Parameters["@json"].Value = json;
                cmdInsertDocument.Parameters["@plainText"].Value = plainText;
                cmdInsertDocument.Parameters["@wordCount"].Value = wordCount;
                cmdInsertDocument.Parameters["@isEncrypted"].Value = isEncrypted;
                cmdInsertDocument.Parameters["@DocumentType"].Value = "Manuscript";
                int insertResult = await cmdInsertDocument.Command.ExecuteNonQueryAsync();
                if (insertResult != 1)
                    return -1;
                // Get ID of new Document.
                long docId = (long)await _cmdGetNewId.ExecuteScalarAsync();
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
