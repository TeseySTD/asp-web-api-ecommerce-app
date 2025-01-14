using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Ordering.Core.Models.Orders.Entities;

public class OrderItem : Entity<OrderItemId>
{
    private OrderItem() : base(){ }
    
    private OrderItem(OrderItemId id, ProductId productId, OrderId orderId, OrderItemQuantity quantity,
        ProductPrice price) : base(id)
    {
        ProductId = productId;
        OrderId = orderId;
        Quantity = quantity;
        Price = price;
    }

    public ProductId ProductId { get; set; }
    public Product Product { get; set; }
    public OrderId OrderId { get; set; }
    public OrderItemQuantity Quantity { get; set; }
    public ProductPrice Price { get; set; }

    public static OrderItem Create(ProductId productId, OrderId orderId, OrderItemQuantity quantity,
        ProductPrice price) =>
        new(OrderItemId.Create(Guid.NewGuid()).Value, productId, orderId, quantity, price);
}