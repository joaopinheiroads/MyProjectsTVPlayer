using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cardápio.Client.Infra.Crypto
{
    public class Crypto
    {
        private readonly byte[] Key;
        private readonly byte[] IV;

        public Crypto()
        {
            Key = GenerateKey(32);
            IV = GenerateKey(16);
        }

        public string Encrypt(string plainText)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using MemoryStream msEncrypt = new MemoryStream();
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            return Base64UrlEncode(msEncrypt.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            byte[] buffer = Base64UrlDecode(cipherText);
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using MemoryStream msDecrypt = new MemoryStream(buffer);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        private static byte[] GenerateKey(int size)
        {
            byte[] key = new byte[size];
            RandomNumberGenerator.Fill(key);
            return key;
        }

        private static string Base64UrlEncode(byte[] input)
        {
            string base64 = Convert.ToBase64String(input);
            return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static byte[] Base64UrlDecode(string input)
        {
            input = input.Replace("-", "+").Replace("_", "/");
            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
            }
            return Convert.FromBase64String(input);
        }
    }
}
