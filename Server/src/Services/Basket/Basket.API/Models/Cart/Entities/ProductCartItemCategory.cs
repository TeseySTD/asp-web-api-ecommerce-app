using System.Text.Json.Serialization;
using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.Domain.Classes;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.Entities;

public class ProductCartItemCategory : Entity<CategoryId>
{
    [JsonInclude]
    public CategoryName CategoryName { get; private set; }
    
    // For Marten
    [JsonConstructor]
    private ProductCartItemCategory() : base(default!) { }

    private ProductCartItemCategory(CategoryId categoryId, CategoryName categoryName) : base(categoryId)
    {
        CategoryName = categoryName;
    }

    public static ProductCartItemCategory Create(CategoryId categoryId, CategoryName categoryName) =>
        new(categoryId, categoryName);
}