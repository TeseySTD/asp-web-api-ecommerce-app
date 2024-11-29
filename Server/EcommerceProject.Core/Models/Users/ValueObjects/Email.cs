using System.Text.RegularExpressions;

namespace EcommerceProject.Core.Models.Users.ValueObjects;

public record Email
{
    public const string RegexEmail = @"^((?!\.)[\w\-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$";

    public string Value { get; init; }

    private Email(string email) => Value = email;

    public static Email Of(string email)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);
        if(Regex.IsMatch(email, RegexEmail))
            return new Email(email);
        else
        {
            throw new FormatException("Email is not valid");
        }
    }
}