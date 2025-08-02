using System.Security.Cryptography;
using System.Text;

namespace CalendarDashboard.Services
{
    public static class AesGcmEncryptor
    {
        public static string encrypt(string value, byte[] key)
        {
            byte[] nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(value);

            byte[] cipherText = new byte[plainTextBytes.Length];

            byte[] tag = new byte[16];

            using (var aes = new AesGcm(key, 16))
            {
                //returns the ciphertext and tag
                aes.Encrypt(nonce, plainTextBytes, cipherText, tag);
            }

            byte[] combined = new byte[nonce.Length + tag.Length + cipherText.Length];

            //combined = nonce + tag + ciphertext
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(cipherText, 0, combined, nonce.Length + tag.Length, cipherText.Length);
            return Convert.ToBase64String(combined);
        }

        public static string decrypt(string value, byte[] key) 
        {
            byte[] combined = Convert.FromBase64String(value);

            //splitting combined into  nonce, tag and ciphertext for decrypting
            byte[] nonce = new byte[12];
            Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);

            byte[] tag = new byte[16];
            Buffer.BlockCopy(combined, nonce.Length, tag, 0, tag.Length);

            byte[] cipherText = new byte[combined.Length - nonce.Length - tag.Length];
            Buffer.BlockCopy(combined, nonce.Length + tag.Length, cipherText, 0, cipherText.Length);

            byte[] plainTextBytes = new byte[cipherText.Length];

            using (var aesGcm = new AesGcm(key, 16))
            {
                //returns original text in bytes
                aesGcm.Decrypt(nonce, cipherText, tag, plainTextBytes);
            }

            return Encoding.UTF8.GetString(plainTextBytes);
        }
    }
}
