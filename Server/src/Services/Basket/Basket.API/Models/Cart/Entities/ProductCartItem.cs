using System.Text.Json.Serialization;
using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.Domain.Classes;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.Entities;

public class ProductCartItem : Entity<ProductId>
{
    [JsonInclude] public ProductTitle Title { get; private set; }
    [JsonInclude] public StockQuantity StockQuantity { get; private set; }
    [JsonInclude] public ProductPrice Price { get; private set; }
    [JsonInclude] public ProductCartItemCategory? Category { get; private set; }
    [JsonInclude] public List<string> ImageUrls { get; private set; } = [];

    // For Marten
    [JsonConstructor]
    private ProductCartItem() : base(default!)
    {
    }

    private ProductCartItem(
        ProductId productId,
        ProductTitle title,
        ProductCartItemCategory category,
        StockQuantity stockQuantity,
        ProductPrice price,
        IEnumerable<string> imageUrls
    ) : base(productId)
    {
        Title = title;
        StockQuantity = stockQuantity;
        Price = price;
        Category = category;
        ImageUrls.AddRange(imageUrls);
    }

    public static ProductCartItem Create(
        ProductId productId,
        ProductTitle title,
        StockQuantity stockQuantity,
        ProductPrice price,
        ProductCartItemCategory category,
        IEnumerable<string> imageUrls = default
    )
    {
        if (imageUrls == null) imageUrls = new List<string>();

        return new ProductCartItem(productId, title, category, stockQuantity, price, imageUrls);
    }

    public void Update(ProductTitle title, ProductPrice price, ProductCartItemCategory? category,
        IEnumerable<string> imageUrls) =>
        (Title, Price, Category, ImageUrls) = (title, price, category, imageUrls.ToList());
}