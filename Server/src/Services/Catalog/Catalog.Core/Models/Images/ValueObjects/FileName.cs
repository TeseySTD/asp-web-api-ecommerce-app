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
            .Check(value.IsNullOrEmpty(), new("File name is required", "File name cannot be null or empty."))
            .DropIfFail()
            .Check(value.Length > MaxLength,
                new("File name is out of range", "File name cannot be longer than " + MaxLength + " characters."))
            .Build();
    }
}