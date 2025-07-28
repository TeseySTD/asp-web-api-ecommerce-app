using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Categories.ValueObjects;

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
                new NameRequiredError())
            .Check(value.Length > MaxNameLength,
                new NameIsOutOfLengthError())
            .Build();
        
        if(result.IsFailure)
            return result;
        return new CategoryName(value);
    }
    
    public sealed record NameRequiredError() : Error($"Name cannot be empty", "Name cannot be empty or whitespace");
    public sealed record NameIsOutOfLengthError() : Error("Name is out of length",$"Name must be less than {MaxNameLength} symbols");
}