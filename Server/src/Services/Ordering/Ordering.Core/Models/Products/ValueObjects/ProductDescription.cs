using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Products.ValueObjects;

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
            .Check(string.IsNullOrEmpty(description),
                new ProductDescriptionRequiredError())
            .Check(description.Length < MinDescriptionLength || description.Length > MaxDescriptionLength,
                new ProductDescriptionOutOfLengthError())
            .Build();

        if (result.IsFailure)
            return result;
        return new ProductDescription(description);
    }

    public sealed record ProductDescriptionRequiredError()
        : Error($"Product description is required", "Product description must not be empty.");

    public sealed record ProductDescriptionOutOfLengthError() : Error("Product description is out of length",
        $"Product description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters.");
}