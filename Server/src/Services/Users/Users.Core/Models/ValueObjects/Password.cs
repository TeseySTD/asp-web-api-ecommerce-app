using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record Password
{
    public const int MinPasswordLength = 6;

    public string Value { get; init; }

    private Password(string password) => Value = password;

    public static Result<Password> Create(string password)
    {
        return Result<Password>.Try(new Password(password))
            .Check(string.IsNullOrEmpty(password),
                new Error("Password is required", "Password must be not null or empty."))
            .DropIfFailed()
            .Check(password.Length < MinPasswordLength,
                new Error("Password less than min length", $"Password less than {MinPasswordLength} characters."))
            .Build();
    }
}