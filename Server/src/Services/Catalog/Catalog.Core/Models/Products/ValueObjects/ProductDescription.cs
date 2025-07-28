using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Products.ValueObjects;

public record ProductDescription
{
    public const int MaxDescriptionLength = 500;
    public const int MinDescriptionLength = 10;
    public string Value { get; private set; }

    private ProductDescription(string description)
    {
        Value = description;
    }

    public static Result<ProductDescription> Create(string description)
    {
        var result = Result<ProductDescription>.Try()
            .Check(string.IsNullOrWhiteSpace(description),
                new DescriptionRequiredError())
            .DropIfFail()
            .Check(description.Length < MinDescriptionLength || description.Length > MaxDescriptionLength,
                new OutOfLengthError())
            .Build();

        if (result.IsFailure)
            return result;
        return new ProductDescription(description);
    }

    public sealed record DescriptionRequiredError()
        : Error($"Product description is required", "Product description cannot be empty");

    public sealed record OutOfLengthError() : Error($"Product description is out of length",
        $"Product description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters.");
}