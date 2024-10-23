using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TjMott.Writer.Models
{
    static class AESHelper
    {
        private const int aesBlockByteSize = 128 / 8;
        private const int aesPasswordSaltByteSize = 128 / 8;
        private const int aesPasswordByteSize = 256 / 8;
        private const int aesPasswordIterationCount = 200_000;

        private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public static string AesEncrypt(string data, string password)
        {
            byte[] byteData = System.Text.Encoding.UTF8.GetBytes(data);
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

                byte[] key = getAesKey(password, keySalt);
                byte[] iv = new byte[aesBlockByteSize];
                _rng.GetBytes(iv);

                using (var encryptor = aes.CreateEncryptor(key, iv))
                {
                    byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
                    result = new byte[aesPasswordSaltByteSize + aesBlockByteSize + encrypted.Length];
                    Array.Copy(keySalt, result, aesPasswordSaltByteSize);
                    Array.Copy(iv, 0, result, aesPasswordSaltByteSize, aesBlockByteSize);
                    Array.Copy(encrypted, 0, result, aesPasswordSaltByteSize + aesBlockByteSize, encrypted.Length);
                }
            }
            return result;
        }

        public static string AesDecrypt(string base64, string password)
        {
            byte[] byteData = Convert.FromBase64String(base64);
            byte[] decrypted = AesDecrypt(byteData, password);
            string retVal = System.Text.Encoding.UTF8.GetString(decrypted);
            return retVal;
        }

        public static byte[] AesDecrypt(byte[] data, string password)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] keySalt = data.Take(aesPasswordSaltByteSize).ToArray();
                byte[] key = getAesKey(password, keySalt);
                byte[] iv = data.Skip(aesPasswordSaltByteSize).Take(aesBlockByteSize).ToArray();
                byte[] encrypted = data.Skip(aesPasswordSaltByteSize + aesBlockByteSize).ToArray();
                byte[] decryptedBytes;

                using (var encryptor = aes.CreateDecryptor(key, iv))
                {
                    decryptedBytes = encryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                }
                return decryptedBytes;
            }
        }

        private static byte[] getAesKey(string password, byte[] passwordSalt)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(password);
            using (var derivator = new Rfc2898DeriveBytes(keyBytes, passwordSalt, aesPasswordIterationCount, HashAlgorithmName.SHA256))
            {
                return derivator.GetBytes(aesPasswordByteSize);
            }
        }
    }
}
