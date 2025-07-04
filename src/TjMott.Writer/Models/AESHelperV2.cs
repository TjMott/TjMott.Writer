using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TjMott.Writer.Models
{
    /// <summary>
    /// This version uses unique randomly-generated data encryption keys for each piece of data,
    /// which are protected with key encryption keys derived from the password.
    /// So it's more correcter than the old version, where the data encryption key
    /// was always derived directly from the password.
    /// </summary>
    static class AESHelperV2
    {
        private const int aesBlockByteSize = 128 / 8;
        private const int ivByteSize = 128 / 8;
        private const int aesPasswordSaltByteSize = 128 / 8;
        private const int aesKeyByteSize = 256 / 8;
        private const int aesPasswordIterationCount = 50_000;

        private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public static string AesEncrypt(string data, string password)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            byte[] encrypted = AesEncrypt(byteData, password);
            string retVal = Convert.ToBase64String(encrypted);
            return retVal;
        }

        public static byte[] AesEncrypt(byte[] data, string password)
        {
            byte[] result;
            using (Aes aes = Aes.Create())
            {
                byte[] keySalt = new byte[aesPasswordSaltByteSize];
                _rng.GetBytes(keySalt);

                // Derive key encryption key from the password and a random salt.
                byte[] keyEncryptionKey = getDerivedKeyEncryptionKey(password, keySalt);
                byte[] keyEncryptionIv = new byte[ivByteSize];
                _rng.GetBytes(keyEncryptionIv);

                // Generate random data encryption key.
                byte[] dataEncryptionKey = new byte[aesKeyByteSize];
                byte[] dataEncryptionIv = new byte[ivByteSize];
                _rng.GetBytes(dataEncryptionKey);
                _rng.GetBytes(dataEncryptionIv);

                // Encrypt DEK with KEK.
                byte[] encryptedDek;
                using (var encryptor = aes.CreateEncryptor(keyEncryptionKey, keyEncryptionIv))
                {
                    byte[] tmp = new byte[dataEncryptionKey.Length + dataEncryptionIv.Length];
                    Array.Copy(dataEncryptionKey, 0, tmp, 0, dataEncryptionKey.Length);
                    Array.Copy(dataEncryptionIv, 0, tmp, dataEncryptionKey.Length, dataEncryptionIv.Length);
                    encryptedDek = encryptor.TransformFinalBlock(tmp, 0, tmp.Length);
                }

                // Now encrypt data with DEK.
                byte[] encryptedData;
                using (var encryptor = aes.CreateEncryptor(dataEncryptionKey, dataEncryptionIv))
                {
                    encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
                }

                // Assemble final array.
                result = new byte[keySalt.Length + keyEncryptionIv.Length + encryptedDek.Length + encryptedData.Length];
                int destIndex = 0;
                Array.Copy(keySalt, 0, result, destIndex, keySalt.Length);
                destIndex += keySalt.Length;
                Array.Copy(keyEncryptionIv, 0, result, destIndex, keyEncryptionIv.Length);
                destIndex += keyEncryptionIv.Length;
                Array.Copy(encryptedDek, 0, result, destIndex, encryptedDek.Length);
                destIndex += encryptedDek.Length;
                Array.Copy(encryptedData, 0, result, destIndex, encryptedData.Length);
            }
            return result;
        }

        public static string AesDecrypt(string base64, string password)
        {
            byte[] byteData = Convert.FromBase64String(base64);
            byte[] decrypted = AesDecrypt(byteData, password);
            string retVal = Encoding.UTF8.GetString(decrypted);
            return retVal;
        }

        public static byte[] AesDecrypt(byte[] data, string password)
        {
            using (Aes aes = Aes.Create())
            {
                // Disassemble the data.
                int index = 0;
                byte[] keySalt = data.Take(aesPasswordSaltByteSize).ToArray();
                index += keySalt.Length;
                byte[] keyEncryptionIv = data.Skip(index).Take(ivByteSize).ToArray();
                index += keyEncryptionIv.Length;
                byte[] encryptedDek = data.Skip(index).Take(64).ToArray();
                index += encryptedDek.Length;
                byte[] encryptedData = data.Skip(index).ToArray();

                // Derive the key encryption key.
                byte[] keyEncryptionKey = getDerivedKeyEncryptionKey(password, keySalt);

                // Decrypt the DEK.
                byte[] dataEncryptionKey = new byte[aesKeyByteSize];
                byte[] dataEncryptionIv = new byte[ivByteSize];
                using (var decryptor = aes.CreateDecryptor(keyEncryptionKey, keyEncryptionIv))
                {
                    byte[] tmp = decryptor.TransformFinalBlock(encryptedDek, 0, encryptedDek.Length);
                    Array.Copy(tmp, 0, dataEncryptionKey, 0, dataEncryptionKey.Length);
                    Array.Copy(tmp, dataEncryptionKey.Length, dataEncryptionIv, 0, dataEncryptionIv.Length);
                }

                // Finally, decrypt the data.
                byte[] decryptedBytes = null;
                using (var decryptor = aes.CreateDecryptor(dataEncryptionKey, dataEncryptionIv))
                {
                    decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }

                return decryptedBytes;
            }
        }

        private static byte[] getDerivedKeyEncryptionKey(string password, byte[] passwordSalt)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(password);
            using (var derivator = new Rfc2898DeriveBytes(keyBytes, passwordSalt, aesPasswordIterationCount, HashAlgorithmName.SHA256))
            {
                return derivator.GetBytes(aesKeyByteSize);
            }
        }
    }
}
