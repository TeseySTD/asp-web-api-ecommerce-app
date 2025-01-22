using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Core.Auth;

public static class Authentication
{
    public static IServiceCollection AddSharedAuthentication(
        this IServiceCollection services,
        string publicKeyPath)
    {
        if (string.IsNullOrEmpty(publicKeyPath))
            throw new InvalidOperationException("JWT_PUBLIC_KEY_PATH is not set");
            
        if (!File.Exists(publicKeyPath))
            throw new FileNotFoundException("Public key file not found", publicKeyPath);
            
        var publicKeyPem = File.ReadAllText(publicKeyPath);
        
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
            });
            
        return services;
    }
}