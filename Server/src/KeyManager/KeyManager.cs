using System.Security.Cryptography;
using System.Text;

namespace KeyManager;

public static class KeyManager
{
    public const int KeySize = 2048;

    public static (string privateKey, string publicKey) GenerateKeys()
    {
        using var rsa = RSA.Create(KeySize);

        var privateKey = ConvertToPem(rsa.ExportRSAPrivateKey(), "RSA PRIVATE KEY");
        var publicKey = ConvertToPem(rsa.ExportRSAPublicKey(), "RSA PUBLIC KEY");

        return (privateKey, publicKey);
    }

    private static string ConvertToPem(byte[] keyBytes, string keyType)
    {
        var base64Key = Convert.ToBase64String(keyBytes);
        var builder = new StringBuilder();
        builder.AppendLine($"-----BEGIN {keyType}-----");
        for (int i = 0; i < base64Key.Length; i += 64)
        {
            builder.AppendLine(base64Key.Substring(i, Math.Min(64, base64Key.Length - i)));
        }
        builder.AppendLine($"-----END {keyType}-----");
        return builder.ToString();
    }
}