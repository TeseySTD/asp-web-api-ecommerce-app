using Shared.Core.Validation;

namespace Users.Core.Models.ValueObjects;

public record HashedPassword
{
    public string Value { get; init; }

    private HashedPassword(string hashedValue)
    {
        Value = hashedValue;
    }

    public static Result<HashedPassword> Create(string hashedValue)
    {
        if (string.IsNullOrEmpty(hashedValue))
            return new Error(nameof(hashedValue), "Hashed password cannot be empty" );
        return new HashedPassword(hashedValue);
    }
}