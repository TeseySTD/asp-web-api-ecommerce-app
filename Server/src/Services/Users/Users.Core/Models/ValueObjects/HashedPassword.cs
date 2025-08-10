using Shared.Core.Validation;
using Shared.Core.Validation.Result;

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
        if (string.IsNullOrEmpty(hashedValue) || string.IsNullOrWhiteSpace(hashedValue))
            return new HashedPasswordEmptyError(); 
        return new HashedPassword(hashedValue);
    }
    
    public sealed record HashedPasswordEmptyError() : Error(nameof(HashedPassword), "Hashed password cannot be empty");
}