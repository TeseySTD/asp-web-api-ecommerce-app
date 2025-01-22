using System.Text;
using KeyManager;

var solutionRootPath = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf("src", StringComparison.Ordinal));
var secretsPath = Path.Combine(solutionRootPath, "secrets");
var privateKeyPath = Path.Combine(secretsPath, "jwt_private_key.pem");
var publicKeyPath = Path.Combine(secretsPath, "jwt_public_key.pem");

if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
{
    Console.WriteLine("Keys not found. Creating new...");
    
    var keys = KeyManager.KeyManager.GenerateKeys();
    SecretManager.SaveKeys(secretsPath, keys.privateKey, keys.publicKey);
}
else
{
    Console.WriteLine("Keys found. Nothing to create new...");
}

var loadedPrivateKey = File.ReadAllText(privateKeyPath, Encoding.UTF8);
var loadedPublicKey = File.ReadAllText(publicKeyPath, Encoding.UTF8);

Console.WriteLine("Keys:");
Console.WriteLine($"Private key (slice):\n {loadedPrivateKey.Substring(0, 50)}...");
Console.WriteLine($"Public key (slice):\n {loadedPublicKey.Substring(0, 50)}...");