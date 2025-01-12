using System.Text.RegularExpressions;
using Shared.Core.Validation;

namespace Users.Core.Models.ValueObjects;

public record PhoneNumber
{
    public const string RegexPhoneNumber = @"^((?:\+?3)?8)?[\s\-\(]*?(0\d{2})[\s\-\)]*?(\d{3})[\s\-]*?(\d{2})[\s\-]*?(\d{2})$";

    public string Value { get; init; }

    private PhoneNumber(string phoneNumber) => Value = phoneNumber;

    public static Result<PhoneNumber> Create(string phoneNumber)
    {
        return Result<PhoneNumber>.TryFail(new PhoneNumber(phoneNumber))
            .CheckError(string.IsNullOrEmpty(phoneNumber),
                new Error("Phone number is required", "Phone number must be not null or empty."))
            .DropIfFailed()
            .CheckError(!Regex.IsMatch(phoneNumber, RegexPhoneNumber),
                new Error("Phone number is incorrect.", "Phone number is not a valid phone number."))
            .Build();
    }
}