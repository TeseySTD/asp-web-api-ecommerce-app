namespace KeyManager;

public static class SecretManager
{
    public static void SaveKeys(string secretsPath, string privateKey, string publicKey)
    {
        var privateKeyPath = Path.Combine(secretsPath, "jwt_private_key.pem");
        var publicKeyPath = Path.Combine(secretsPath, "jwt_public_key.pem");
        
        Directory.CreateDirectory(secretsPath);

        File.WriteAllText(privateKeyPath, privateKey);
        File.WriteAllText(publicKeyPath, publicKey);

        Console.WriteLine("Keys have been saved to files.");
    }
}