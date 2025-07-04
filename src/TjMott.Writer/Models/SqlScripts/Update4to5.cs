using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TjMott.Writer.ViewModels;
using TjMott.Writer.Views;

namespace TjMott.Writer.Models.SqlScripts
{
    /// <summary>
    /// This updater does the following changes:
    ///   1. Improves the document encryption scheme to use random data encryption keys which
    ///      are protected by a password-derived key encryption key.
    ///   2. Deprecates the notes feature, and migrates all old notes to be scenes.
    /// </summary>
    public class Update4to5 : DbUpgrader
    {
        private static string _script4to5 = @"

    DROP TRIGGER NoteDocument_ad;
    DROP TRIGGER NoteDocument_ai_fts;
    DROP TRIGGER NoteDocument_ad_fts;
    DROP TRIGGER NoteDocument_au_fts;

    DROP TABLE NoteDocument_fts;
    DROP TABLE NoteCategoryDocument;
    DROP TABLE NoteCategory;
    DROP TABLE NoteDocument;

    DROP TRIGGER Story_NoteDoc_ad; -- Wasn't named appropriately.
    CREATE TRIGGER Story_CopyrightPageDoc_ad AFTER DELETE ON Story BEGIN
      DELETE FROM Document WHERE id = (old.CopyrightPageId);
    END;

    UPDATE Metadata SET Value = 5 WHERE Key = 'DbVersion';
";

        private SqliteConnection _connection;
        private Window _dialogOwner;

        public Update4to5()
        {
            StartVersion = 4;
            TargetVersion = 5;
        }

        public override async Task<bool> DoUpgradeAsync(SqliteConnection connection, Window dialogOwner)
        {
            _connection = connection;
            _dialogOwner = dialogOwner;

            bool result = await migrateNoteDocuments();
            if (!result) return result;

            result = await upgradeDocumentEncryption();
            if (!result) return result;

            result = await runScriptWithVersionCheckAsync(_connection, _script4to5, 4, 5);
            return result;
        }

        private async Task<bool> migrateNoteDocuments()
        {
            using var getNewIdCmd = new SqliteCommand("select last_insert_rowid()", _connection);
            using var getCmd = new SqliteCommand("SELECT id, UniverseId, DocumentId, Name FROM NoteDocument", _connection);

            using var createStoryCmd = new DbCommandHelper(_connection);
            createStoryCmd.Command.CommandText = "INSERT INTO Story (UniverseId, Name) VALUES (@universeId, 'Migrated Notes');";
            createStoryCmd.AddParameter("@universeId");

            using var createChapterCmd = new DbCommandHelper(_connection);
            createChapterCmd.Command.CommandText = "INSERT INTO Chapter (StoryId, Name) VALUES (@storyId, 'Migrated Notes');";
            createChapterCmd.AddParameter("@storyId");

            using var createSceneCmd = new DbCommandHelper(_connection);
            createSceneCmd.Command.CommandText = "INSERT INTO Scene (ChapterId, Name, DocumentId) VALUES (@chapterId, @name, @documentId);";
            createSceneCmd.AddParameter("@chapterId");
            createSceneCmd.AddParameter("@name");
            createSceneCmd.AddParameter("@documentId");

            Dictionary<long, long> universeToNoteStoryMapping = new Dictionary<long, long>();
            Dictionary<long, long> universeToNoteChapterMapping = new Dictionary<long, long>();

            // Enumerate all note documents.
            using (var noteReader = await getCmd.ExecuteReaderAsync())
            {
                while (await noteReader.ReadAsync())
                {
                    long noteId = noteReader.GetInt64(0);
                    long universeId = noteReader.GetInt64(1);
                    long documentId = noteReader.GetInt64(2);
                    string noteName = noteReader.GetString(3);

                    // If we don't have a migrated notes story/chapter for this universe, create it.
                    if (!universeToNoteStoryMapping.ContainsKey(universeId))
                    {
                        createStoryCmd.Parameters["@universeId"].Value = universeId;
                        if (await createStoryCmd.Command.ExecuteNonQueryAsync() == 0)
                            return false;

                        long storyId = (long)await getNewIdCmd.ExecuteScalarAsync();
                        universeToNoteStoryMapping[universeId] = storyId;

                        createChapterCmd.Parameters["@storyId"].Value = storyId;
                        if (await createChapterCmd.Command.ExecuteNonQueryAsync() == 0)
                            return false;

                        long chapterId = (long)await getNewIdCmd.ExecuteScalarAsync();
                        universeToNoteChapterMapping[universeId] = chapterId;
                    }

                    // Create a new scene for this note.
                    createSceneCmd.Parameters["@chapterId"].Value = universeToNoteChapterMapping[universeId];
                    createSceneCmd.Parameters["@name"].Value = noteName;
                    createSceneCmd.Parameters["@documentId"].Value = documentId;

                    if (await createSceneCmd.Command.ExecuteNonQueryAsync() == 0)
                        return false;
                }
            }

            return true;
        }

        private async Task<bool> upgradeDocumentEncryption()
        {
            using var getCommand = new SqliteCommand("SELECT id, Json FROM Document WHERE IsEncrypted = 1", _connection);

            using var updateCommand = new DbCommandHelper(_connection);
            updateCommand.Command.CommandText = "UPDATE Document SET Json = @json WHERE id = @id";
            updateCommand.AddParameter("@json");
            updateCommand.AddParameter("@id");

            List<string> aesPasswords = new List<string>();

            // Search for all encrypted documents.
            using (SqliteDataReader reader = await getCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    long id = reader.GetInt64(0);
                    string encryptedJson = reader.GetString(1);
                    string decryptedJson = null;
                    string itemPassword = null;

                    while (aesPasswords.Count == 0)
                        aesPasswords.Add(await promptForPassword());

                    // Attempt to decrypt with known passwords.
                    bool decrypted = false;
                    foreach (var password in aesPasswords)
                    {
                        try
                        {
#pragma warning disable CS0612 // Type or member is obsolete
                            decryptedJson = AESHelper.AesDecrypt(encryptedJson, password);
#pragma warning restore CS0612 // Type or member is obsolete
                            decrypted = true;
                            itemPassword = password;
                        }
                        catch (CryptographicException) { }
                    }

                    // Known passwords didn't work, prompt for more.
                    while (!decrypted)
                    {
                        aesPasswords.Add(await promptForPassword());

                        try
                        {
#pragma warning disable CS0612 // Type or member is obsolete
                            decryptedJson = AESHelper.AesDecrypt(encryptedJson, aesPasswords.Last());
#pragma warning restore CS0612 // Type or member is obsolete
                            decrypted = true;
                            itemPassword = aesPasswords.Last();
                        }
                        catch (CryptographicException) { }
                    }

                    // Re-encrypt the data using the same password but the new AES code.
                    string encrypted = AESHelperV2.AesEncrypt(decryptedJson, itemPassword);

                    // Save back to database.
                    updateCommand.Parameters["@id"].Value = id;
                    updateCommand.Parameters["@json"].Value = encrypted;
                    int updateResult = await updateCommand.Command.ExecuteNonQueryAsync();
                    if (updateResult == 0)
                        return false;
                }
            }

            return true;
        }

        private async Task<string> promptForPassword()
        {
            while (true)
            {
                PasswordInputViewModel passwordInputViewModel = new PasswordInputViewModel();
                PasswordInputView passwordInputView = new PasswordInputView()
                {
                    DataContext = passwordInputViewModel
                };

                passwordInputViewModel.Prompt = "A password is required to unlock a Document.";
                await passwordInputView.ShowDialog(_dialogOwner);
                if (passwordInputViewModel.DialogAccepted)
                {
                    return passwordInputViewModel.Password;
                }
            }
        }
    }
}
