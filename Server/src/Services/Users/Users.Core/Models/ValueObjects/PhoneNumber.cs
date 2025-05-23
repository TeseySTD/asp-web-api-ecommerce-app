using System.Text.RegularExpressions;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record PhoneNumber
{
    public const string RegexPhoneNumber = @"^((?:\+?3)?8)?[\s\-\(]*?(0\d{2})[\s\-\)]*?(\d{3})[\s\-]*?(\d{2})[\s\-]*?(\d{2})$";

    public string Value { get; init; }

    private PhoneNumber(string phoneNumber) => Value = phoneNumber;

    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        return Result<PhoneNumber>.Try(new PhoneNumber(phoneNumber))
            .Check(string.IsNullOrEmpty(phoneNumber) || string.IsNullOrWhiteSpace(phoneNumber),
                new Error("Phone number is required", "Phone number must be not null or empty."))
            .DropIfFail()
            .Check(!Regex.IsMatch(phoneNumber, RegexPhoneNumber),
                new Error("Phone number is incorrect.", "Phone number is not a valid phone number."))
            .Build();
    }
}