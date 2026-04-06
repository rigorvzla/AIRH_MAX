using System.IO;
using System.Security.Cryptography;

namespace AIRH_MAX.ClassView
{
    public static class SecureText
    {
        private const int SALT_SIZE = 16;
        private const int IV_SIZE = 16;
        private const int KEY_SIZE = 32; // 256 bits
        private const int ITERATIONS = 10000;

        /// <summary>
        /// Desencripta un texto usando la misma contraseña.
        /// </summary>
        /// <param name="encryptedText">Texto en Base64</param>
        /// <param name="password">Contraseña usada al encriptar</param>
        /// <returns>Texto original o null si falla</returns>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return null;

            try
            {
                var fullCipher = Convert.FromBase64String(encryptedText);

                using var ms = new MemoryStream(fullCipher);
                var salt = new byte[SALT_SIZE];
                var iv = new byte[IV_SIZE];

                ms.Read(salt, 0, salt.Length);
                ms.Read(iv, 0, iv.Length);

                var key = new Rfc2898DeriveBytes("af74e0f2-3701-4005-8df2-736108dfd101", salt, ITERATIONS, HashAlgorithmName.SHA256);
                var aesKey = key.GetBytes(KEY_SIZE);

                using var aes = Aes.Create();
                aes.Key = aesKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception)
            {
                // Cifrado inválido, contraseña incorrecta, etc.
                return null;
            }
        }
    }
}