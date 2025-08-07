using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Categories.ValueObjects;

public record CategoryDescription
{
    public const int MinDescriptionLength = 3;
    public const int MaxDescriptionLength = 200;
    
    private CategoryDescription(string value)
    {
        Value = value;
    }

    public string Value { get; private init; }

    public static Result<CategoryDescription> Create(string value)
    {
        return Result<CategoryDescription>.Try(new CategoryDescription(value))
            .Check(string.IsNullOrWhiteSpace(value),
                new DescriptionRequiredError())
            .Check(value.Length < MinDescriptionLength || value.Length > MaxDescriptionLength,
                new OutOfLengthError())
            .Build();
    }
    
    public sealed record DescriptionRequiredError() : Error ("Description is required.", " Description cannot be whitespace or empty.");
    public sealed record OutOfLengthError() : Error("Description is out of range.", $"Description cannot be less than {MaxDescriptionLength} characters and more than {MinDescriptionLength} symbols.");
}