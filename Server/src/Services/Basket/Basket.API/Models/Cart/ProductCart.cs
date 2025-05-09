using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Basket.API.Models.Cart;

public class ProductCart : AggregateRoot<UserId>
{
    public List<ProductCartItem> Items { get; private set; } = [];
    public decimal TotalPrice => Items.Sum(x => x.Price.Value * x.StockQuantity.Value);

    private ProductCart(UserId userId) : base(userId){}
    // For Marten
    private ProductCart() : base(default!) { }
    
    public void AddItem(ProductCartItem item) => Items.Add(item);
    public void AddItems(IEnumerable<ProductCartItem> items) => Items.AddRange(items);
    public void RemoveItem(ProductId productId) => Items.RemoveAll(x => x.Id == productId);
    public bool HasItem(ProductId productId) => Items.Any(x => x.Id == productId);

    public static ProductCart Create(UserId userId) => new(userId);
    
}

