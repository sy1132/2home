using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AesHelper
{
    // Bạn nên lưu key và IV an toàn, không nên hard-code như dưới. Chỉ dùng cho demo!
    private static readonly string KeyString = "A1B2C3D4E5F6G7H8"; // 16 ký tự (128 bit)
    private static readonly string IVString = "I1J2K3L4M5N6O7P8";  // 16 ký tự (128 bit)

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(KeyString);
            aes.IV = Encoding.UTF8.GetBytes(IVString);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(KeyString);
            aes.IV = Encoding.UTF8.GetBytes(IVString);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] inputBytes = Convert.FromBase64String(cipherText);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
