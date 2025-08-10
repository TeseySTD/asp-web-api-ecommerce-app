namespace Users.Infrastructure.Authentication;

public sealed class EmailVerificationTokenSettings
{
    public static string SectionName { get; } = "EmailVerificationTokenSettings";

    public uint ExpirationInMinutes { get; init; }
};