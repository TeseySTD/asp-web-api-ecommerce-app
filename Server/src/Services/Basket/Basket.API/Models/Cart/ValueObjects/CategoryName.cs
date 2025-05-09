using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public record CategoryName
{
    public const int MaxNameLength = 50;

    private CategoryName(string value)
    {
        Value = value;
    }

    public string Value { get; private init; }

    public static Result<CategoryName> Create(string value)
    {
        var result = Result<CategoryName>.Try()
            .Check(string.IsNullOrWhiteSpace(value),
                new Error("Name cannot be null or whitespace", nameof(CategoryName)))
            .Check(value.Length > MaxNameLength,
                new Error($"Name must be less than {MaxNameLength} symbols", nameof(CategoryName)))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new CategoryName(value);
    }
    
}