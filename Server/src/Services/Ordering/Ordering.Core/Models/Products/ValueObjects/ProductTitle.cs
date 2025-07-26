using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Core.Models.Products.ValueObjects;

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
        var result = Result<ProductTitle>.Try()
            .Check(string.IsNullOrEmpty(title), 
                new ProductTitleRequiredError())
            .DropIfFail()
            .Check(title.Length > MaxTitleLength || title.Length < MinTitleLength,
                new ProductTitleOutOfRangeError())
            .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductTitle(title);
    }

    public sealed record ProductTitleRequiredError() : Error($"Product title is required", "Title must be not empty");
    public sealed record ProductTitleOutOfRangeError() : Error($"Product title is out of range", $"Product title must be between {MinTitleLength} and {MaxTitleLength} characters");
}
