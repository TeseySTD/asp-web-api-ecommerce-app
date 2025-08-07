using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Core.Models.Categories.ValueObjects;

public record CategoryId
{
    private CategoryId(Guid categoryId)
    {
        Value = categoryId;
    }

    public Guid Value { get; set; }

    public static Result<CategoryId> Create(Guid categoryId)
    {
        return Result<CategoryId>.Try(new CategoryId(categoryId))
            .Check(categoryId == Guid.Empty, new IdIsRequiredError())
            .Build();
    }
    
    public sealed record IdIsRequiredError() : Error($"Category Id is required", "CategoryId cannot be empty.");
}