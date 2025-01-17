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
        var result = Result<CategoryId>.Try()
            .Check(categoryId == Guid.Empty,
                new Error("Category Id is invalid",  nameof(CategoryId)))
            .Build();
        
        if(result.IsFailure)
            return result;
        return new CategoryId(categoryId);
    }
}