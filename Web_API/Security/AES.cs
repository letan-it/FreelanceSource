using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Web_API
{
    public class AES
    {
        public static string EncryptAES(string AppID, string HealthMeasurementKey)
        {
            try
            {
                byte[] encrypted;
                var aesIv = "";
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {
                    aes.Key = Convert.FromBase64String(HealthMeasurementKey);
                    aes.GenerateIV();
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;



                    aesIv = Convert.ToBase64String(aes.IV);



                    ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);



                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(AppID);
                            }



                            encrypted = ms.ToArray();
                        }
                    }



                }
                return $"{aesIv}!{Convert.ToBase64String(encrypted)}";
            }
            catch
            {
                return null;
            }
        }

        public static string DecryptAES(string encryptedText, string aesKey)
        {
            try
            {
                var aesIv = encryptedText.Substring(0, encryptedText.IndexOf("!"));
                encryptedText = encryptedText.Substring(encryptedText.IndexOf("!") + 1);

                string decrypted = null;
                byte[] cipher = Convert.FromBase64String(encryptedText);

                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                {
                    aes.Key = Convert.FromBase64String(aesKey);
                    aes.IV = Convert.FromBase64String(aesIv);
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(cipher))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                decrypted = sr.ReadToEnd();
                            }
                        }
                    }
                }

                return decrypted;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
