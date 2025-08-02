using System.Security.Cryptography;

public static class EncryptionUtils
{
    public static byte[] GenerateRandomKey()
    {
        byte[] key = new byte[32]; // 256 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return key;
    }
}