using Microsoft.IdentityModel.Tokens;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Images.ValueObjects;

public record FileName
{
    public string Value { get; init; }
    public const int MaxLength = 255;

    private FileName(string value)
    {
        Value = value;
    }

    public static Result<FileName> Create(string value)
    {
        return Result<FileName>.Try(new FileName(value))
            .Check(string.IsNullOrWhiteSpace(value), new FileNameRequiredError())
            .DropIfFail()
            .Check(value.Length > MaxLength, new OutOfLengthError())
            .Build();
    }

    public sealed record FileNameRequiredError()
        : Error("File name is required", "File name cannot be whitespace or empty.");

    public sealed record OutOfLengthError() : Error("File name is out of length.",
        "File name cannot be longer than " + MaxLength + " characters.");
}