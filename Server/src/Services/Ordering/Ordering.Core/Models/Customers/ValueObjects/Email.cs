using System.Text.RegularExpressions;
using Shared.Core.Validation;

namespace Ordering.Core.Models.Customers.ValueObjects;

public record Email
{
    public const string RegexEmail = @"^((?!\.)[\w\-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$";

    public string Value { get; init; }

    private Email(string email) => Value = email;

    public static Result<Email> Create(string email)
    {
        return Result<Email>.TryFail(new Email(email))
            .CheckError(string.IsNullOrEmpty(email),
                new Error("Email is required", "Email cannot be null or empty"))
            .DropIfFailed()
            .CheckError(!Regex.IsMatch(email, RegexEmail),
                new Error("Email is not a valid email", "Email is not a valid email"))
            .Build();
    }
}