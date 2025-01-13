using Shared.Core.Validation;

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
        var result = Result<ProductTitle>.TryFail()
            .CheckError(string.IsNullOrEmpty(title), 
                new Error("Title is required", "Title must be not empty"))
            .CheckError(title.Length > MaxTitleLength || title.Length < MinTitleLength,
                new Error("Title is too long", $"Product title must be between {MinTitleLength} and {MaxTitleLength} characters"))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductTitle(title);
    }

}
