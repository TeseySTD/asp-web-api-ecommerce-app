using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

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
                new Error("Title is required", "Title must be not empty"))
            .Check(title.Length > MaxTitleLength || title.Length < MinTitleLength,
                new Error("Title is too long", $"Product title must be between {MinTitleLength} and {MaxTitleLength} characters"))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductTitle(title);
    }
}