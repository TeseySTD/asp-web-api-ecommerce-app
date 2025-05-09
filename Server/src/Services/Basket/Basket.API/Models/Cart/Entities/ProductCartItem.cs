using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.Domain.Classes;
using Shared.Core.Validation.Result;

namespace Basket.API.Models.Cart.Entities;

public class ProductCartItem : Entity<ProductId>
{
    public StockQuantity StockQuantity { get; private set; }
    public ProductPrice Price { get; private set; }
    public ProductCartItemCategory Category { get; private set; }
    public List<string> ImageUrls { get; private set; } = [];

    private ProductCartItem(
        ProductId productId,
        ProductCartItemCategory category,
        StockQuantity stockQuantity,
        ProductPrice price,
        IEnumerable<string> imageUrls
    ) : base(productId)
    {
        StockQuantity = stockQuantity;
        Price = price;
        Category = category;
        ImageUrls.AddRange(imageUrls);
    }

    public static ProductCartItem Create(
        ProductId productId,
        StockQuantity stockQuantity,
        ProductPrice price,
        ProductCartItemCategory category,
        IEnumerable<string> imageUrls = default
    )
    {
        if (imageUrls == null) imageUrls = new List<string>();
        
        return new ProductCartItem(productId, category, stockQuantity, price, imageUrls);
    }
}