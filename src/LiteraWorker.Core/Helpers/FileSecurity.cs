using System.Security.Cryptography;

namespace LiteraWorker.Core.Helpers;

public static class FileSecurity
{
    private static readonly byte[] _key =
    [
        0xBE, 0xB9, 0x02, 0xC3, 0x40, 0x41, 0xC2, 
        0xED, 0xDF, 0x71, 0x85, 0x41, 0x1B, 0xCB, 
        0x45, 0x72, 0x91, 0xB9, 0xB5, 0x3A, 0xBD, 
        0xA4,0x94, 0xBE, 0x3C, 0x82, 0x07, 0x44, 
        0x0D, 0x72, 0x13, 0xED
    ];

    public static void Encrypt(string filePath, string content)
    {
        using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write);

        using var aes = Aes.Create();
        aes.Key = _key;

        stream.Write(aes.IV);

        using var crypto = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using var writer = new StreamWriter(crypto);

        writer.Write(content);
    }

    public static string Decrypt(string filePath)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var aes = Aes.Create();

        aes.Key = _key;

        byte[] iv = new byte[aes.BlockSize / 8];
        stream.ReadExactly(iv, 0, iv.Length);
        aes.IV = iv;

        using var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream);
        return reader.ReadToEnd();
    }
}
