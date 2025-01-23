using System.Text.RegularExpressions;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record Email
{
    public const string RegexEmail = @"^((?!\.)[\w\-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$";

    public string Value { get; init; }

    private Email(string email) => Value = email;

    public static Result<Email> Create(string email)
    {
        return Result<Email>.Try(new Email(email))
            .Check(string.IsNullOrEmpty(email),
                new Error("Email is required", "Email cannot be null or empty"))
            .DropIfFail()
            .Check(!Regex.IsMatch(email, RegexEmail),
                new Error("Email is not a valid email", "Email is not a valid email"))
            .Build();
    }
}