using Camera_Monitoring_BaVPL.Core.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Camera_Monitoring_BaVPL.Core.Services
{
    public class EncryptionService : IEncryptionService
    {
        private static readonly string EncryptionKey = "H0w3st-B@VPL-2O24#"; 

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return null;

            using (Aes aes = Aes.Create())
            {
                aes.GenerateIV(); 
                byte[] iv = aes.IV;
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32));

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv);

                using (var ms = new MemoryStream())
                {
                    
                    ms.Write(iv, 0, iv.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return null;

            byte[] fullCipher = Convert.FromBase64String(encryptedText);

            using (Aes aes = Aes.Create())
            {

                byte[] iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);

                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32));
                aes.IV = iv; 

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
