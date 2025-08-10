using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Products.ValueObjects;

public record ProductTitle
{
    public const int MaxTitleLength = 200;
    public const int MinTitleLength = 2;
    public string Value { get; }

    protected ProductTitle(string title)
    {
        Value = title;
    }

    public static Result<ProductTitle> Create(string title)
    {
        return Result<ProductTitle>.Try(new ProductTitle(title))
            .Check(string.IsNullOrWhiteSpace(title), 
                new TitleRequiredError())
            .Check(title.Length > MaxTitleLength || title.Length < MinTitleLength,
                new OutOfLengthError()) 
            .Build();
    }

    // For EF Core queries
    public static explicit operator string(ProductTitle productTitle) => productTitle.Value;
    
    public sealed record TitleRequiredError() : Error("Title is required", "Title must be not empty");
    public sealed record OutOfLengthError() : Error("Title is out of range.", $"Product title must be between {MinTitleLength} and {MaxTitleLength} characters.");
}
