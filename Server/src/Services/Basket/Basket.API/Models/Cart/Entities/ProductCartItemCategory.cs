using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.Domain.Classes;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.Entities;

public class ProductCartItemCategory : Entity<CategoryId>
{
    public CategoryName CategoryName { get; set; }

    private ProductCartItemCategory(CategoryId categoryId, CategoryName categoryName) : base(categoryId)
    {
        CategoryName = categoryName;
    }

    public static ProductCartItemCategory Create(CategoryId categoryId, CategoryName categoryName) =>
        new(categoryId, categoryName);
}