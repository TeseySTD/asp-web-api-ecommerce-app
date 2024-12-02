namespace EcommerceProject.Core.Models.Users.ValueObjects;

public record Password
{
    public const int MinPasswordLength = 6;

    public string Value { get; init; }

    private Password(string password) => Value = password;

    public static Password Create(string password)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(password.Length, MinPasswordLength, nameof(password));
        return new Password(password);
    }
}