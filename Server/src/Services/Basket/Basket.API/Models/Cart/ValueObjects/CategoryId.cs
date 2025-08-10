using System.Text.Json.Serialization;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.ValueObjects;

public record CategoryId
{
    private CategoryId(Guid categoryId)
    {
        Value = categoryId;
    }
    [JsonInclude]
    public Guid Value { get; private set; }
    // For Marten
    [JsonConstructor]
    private CategoryId() { }
    public static CategoryId From(Guid value) => new(value);
    public static Result<CategoryId> Create(Guid categoryId)
    {
        return Result<CategoryId>.Try(new CategoryId(categoryId))
            .Check(categoryId == Guid.Empty, new IdIsRequiredError())
            .Build();
    }
    
    public sealed record IdIsRequiredError() : Error($"Category Id is required", "CategoryId cannot be empty.");

}