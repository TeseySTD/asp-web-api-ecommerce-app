using System.Text.RegularExpressions;
using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Orders.ValueObjects;

public record Address
{
    public const string RegexZipCode = "^[0-9]{5}(?:-[0-9]{4})?$";
    public string AddressLine { get; init; } = default!;
    public string Country { get; init; } = default!;
    public string State { get; init; } = default!;
    public string ZipCode { get; init; } = default!;

    private Address(string addressLine, string? country, string? state, string? zipCode)
    {
        AddressLine = addressLine;
        Country = country ?? "Unknown";
        State = state ?? "Unknown";
        ZipCode = zipCode ?? "Unknown";
    }

    public static Result<Address> Create(string addressLine, string? country, string? state, string? zipCode)
    {
        return Result<Address>.Try(new Address(addressLine, country, state, zipCode))
            .Check(string.IsNullOrWhiteSpace(addressLine),
                new AddressLineRequiredError())
            .CheckIf(
                checkCondition: !string.IsNullOrWhiteSpace(zipCode), 
                () => !Regex.IsMatch(zipCode!, RegexZipCode),
                error: new ZipCodeFormatError(zipCode ?? ""))
            .Build();
    }

    public sealed record AddressLineRequiredError()
        : Error("Address line is required", "Address line cannot be whitespace or empty");

    public sealed record ZipCodeFormatError(string ZipCode)
        : Error("Zip code is invalid", $"Zip code '{ZipCode}' is invalid.");
}
