using System.Text.RegularExpressions;

namespace EcommerceProject.Core.Models.Users.ValueObjects;

public record PhoneNumber
{
    public const string RegexPhoneNumber = @"^((?:\+?3)?8)?[\s\-\(]*?(0\d{2})[\s\-\)]*?(\d{3})[\s\-]*?(\d{2})[\s\-]*?(\d{2})$";

    public string Value { get; init; }

    private PhoneNumber(string phoneNumber) => Value = phoneNumber;

    public static PhoneNumber Create(string phoneNumber)
    {
        if(Regex.IsMatch(phoneNumber, RegexPhoneNumber))
            return new PhoneNumber(phoneNumber);
        else
        {
            throw new FormatException("Invalid phone number");
        }
    }
}