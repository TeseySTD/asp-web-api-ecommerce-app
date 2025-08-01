using System.Text.Json.Serialization;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public record ProductTitle
{
    public const int MaxTitleLength = 200;
    public const int MinTitleLength = 2;
    [JsonInclude]
    public string Value { get; private set; }
    
    // For Marten
    [JsonConstructor]
    private ProductTitle() { }
    protected ProductTitle(string title)
    {
        Value = title;
    }

    public static Result<ProductTitle> Create(string title)
    {
        var result = Result<ProductTitle>.Try()
            .Check(string.IsNullOrWhiteSpace(title), 
                new TitleRequiredError())
            .Check(title.Length > MaxTitleLength || title.Length < MinTitleLength,
                new OutOfLengthError()) 
            .Build();
        
        if(result.IsFailure)
            return result;
        return new ProductTitle(title);
    }

    public sealed record TitleRequiredError() : Error("Title is required", "Title must be not empty");
    public sealed record OutOfLengthError() : Error("Title is out of range.", $"Product title must be between {MinTitleLength} and {MaxTitleLength} characters.");
}