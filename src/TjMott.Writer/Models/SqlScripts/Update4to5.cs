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
    /// The database schema didn't change here, but the document encryption schem
    /// was improved. So this updater re-encrypts any encrypted documents.
    /// </summary>
    public class Update4to5 : DbUpgrader
    {
        private static string _script4to5 = @"
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

            bool result = await upgradeDocumentEncryption();

            result = await runScriptWithVersionCheckAsync(_connection, _script4to5, 4, 5);
            return result;
        }

        private async Task<bool> upgradeDocumentEncryption()
        {
            using var getCommand = new SqliteCommand("SELECT id, Json FROM Document WHERE IsEncrypted = 1", _connection);
            using var updateCommand = new SqliteCommand("UPDATE Document SET Json = @json WHERE id = @id", _connection);
            SqliteParameter jsonParameter = new SqliteParameter();
            jsonParameter.ParameterName = "@json";
            updateCommand.Parameters.Add(jsonParameter);
            SqliteParameter idParameter = new SqliteParameter();
            idParameter.ParameterName = "@id";
            updateCommand.Parameters.Add(idParameter);

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
                    idParameter.Value = id;
                    jsonParameter.Value = encrypted;
                    int updateResult = await updateCommand.ExecuteNonQueryAsync();
                }
            }

            return false;
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
