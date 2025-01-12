namespace Users.Infrastructure.Authentication;

public sealed class RefreshTokenSettings
{
    public static string SectionName { get; } = "RefreshTokenSettings";

    public uint ExpirationInDays { get; init; }

}