using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class SecurityUtils
    {
        /// <summary>
        /// Encrypts specified plaintext using Rijndael symmetric key algorithm
        /// and returns a base64-encoded result.
        /// </summary>
        /// <param name="plainText">
        /// Plaintext value to be encrypted.
        /// </param>
        /// <param name="passPhrase">
        /// Passphrase from which a pseudo-random password will be derived. The
        /// derived password will be used to generate the encryption key.
        /// Passphrase can be any string. In this example we assume that this
        /// passphrase is an ASCII string.
        /// </param>
        /// <param name="saltValue">
        /// Salt value used along with passphrase to generate password. Salt can
        /// be any string. In this example we assume that salt is an ASCII string.
        /// </param>
        /// <param name="hashAlgorithm">
        /// Hash algorithm used to generate password. Allowed values are: "MD5" and
        /// "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
        /// </param>
        /// <param name="passwordIterations">
        /// Number of iterations used to generate password. One or two iterations
        /// should be enough.
        /// </param>
        /// <param name="initVector">
        /// Initialization vector (or IV). This value is required to encrypt the
        /// first block of plaintext data. For RijndaelManaged class IV must be 
        /// exactly 16 ASCII characters long.
        /// </param>
        /// <param name="keySize">
        /// Size of encryption key in bits. Allowed values are: 128, 192, and 256. 
        /// Longer keys are more secure than shorter keys.
        /// </param>
        /// <returns>
        /// Encrypted value formatted as a base64-encoded string.
        /// </returns>
        private static string Encrypt(string plainText, string passPhrase, string saltValue, string initVector, string hashAlgorithm = "SHA1", int passwordIterations = 2, int keySize = 256)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherTextBytes);
            return cipherText;
        }

        private static string Decrypt(string cipherText, string passPhrase, string saltValue, string initVector, string hashAlgorithm = "SHA1", int passwordIterations = 2, int keySize = 256)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);

            string dummyData = cipherText.Trim().Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');

            byte[] cipherTextBytes = Convert.FromBase64String(dummyData);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
            byte[] keyBytes = password.GetBytes(keySize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            string plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            return plainText;
        }
        public static string Encrypt(string data, string passPhrase, string saltValue, string keyData)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new Exception("Encrypt: Data not null.");
            if (string.IsNullOrEmpty(keyData))
                throw new Exception("Encrypt: Key not null.");
            keyData = keyData.PadRight(16, '0');
            if (keyData.Length > 16)
                keyData = keyData.Substring(0, 16);
            return SecurityUtils.Encrypt(data, passPhrase, saltValue, keyData, "SHA1");
        }
        public static string Encrypt(string data, string passPhrase, string keyData)
        {
            return Encrypt(data, passPhrase, "Dicentral", keyData);
        }
        public static string Decrypt(string data, string passPhrase, string saltValue, string keyData)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new Exception("Encrypt: Data not null.");
            if (string.IsNullOrEmpty(keyData))
                throw new Exception("Encrypt: Key not null.");
            keyData = keyData.PadRight(16, '0');
            if (keyData.Length > 16)
                keyData = keyData.Substring(0, 16);
            return SecurityUtils.Decrypt(data, passPhrase, saltValue, keyData, "SHA1");
        }
        public static string Decrypt(string data, string passPhrase, string keyData)
        {
            return Decrypt(data, passPhrase, "Dicentral", keyData);
        }
        private static string passPhrase = "DicentralpassPhrase";
        private static string keyData = "DicentralkeyData";
        public static string Decrypt(string data)
        {
            return Decrypt(data, passPhrase, "Dicentral@2022", keyData);
        }
        public static string Encrypt(string data)
        {
            return Encrypt(data, passPhrase, "Dicentral@2022", keyData);
        }

    }
}
