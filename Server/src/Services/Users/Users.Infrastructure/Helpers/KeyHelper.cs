using System.Security.Cryptography;

namespace Users.Infrastructure.Helpers;

public static class KeyHelper
{
    public static RSA GetKey(string keyEnvVariableName)
    {
        var publicKeyPath = Environment.GetEnvironmentVariable(keyEnvVariableName);
        
        if (string.IsNullOrEmpty(publicKeyPath))
            throw new InvalidOperationException($"{keyEnvVariableName} is not set");
            
        if (!File.Exists(publicKeyPath))
            throw new FileNotFoundException("Key file not found", publicKeyPath);
            
        var publicKeyPem = File.ReadAllText(publicKeyPath);
        
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        
        return rsa;
    }
}