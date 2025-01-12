namespace Users.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public static string SectionName { get; } = "JwtSettings";
    
    public uint ExpirationInMinutes { get; init; }
    public string SecretKey { get; init; } = null!;
}