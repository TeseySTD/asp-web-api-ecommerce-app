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
        var result = Result<CategoryDescription>.Try()
            .Check(string.IsNullOrEmpty(value),
                new Error("Value cannot be null or empty.", nameof(value)))
            .Check(value.Length < MinDescriptionLength || value.Length > MaxDescriptionLength,
                new Error("Description is out of range.",
                    $"Description must be less then {MaxDescriptionLength} symbols and more than {MinDescriptionLength} symbols"))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new CategoryDescription(value);
    }
}