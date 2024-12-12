using System;
using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Core.Models.Orders.Entities;

public class OrderItem : Entity<OrderItemId>
{
    private OrderItem(OrderItemId id, ProductId productId, OrderId orderId, OrderItemQuantity quantity,
        ProductPrice price) : base(id)
    {
        ProductId = productId;
        OrderId = orderId;
        Quantity = quantity;
        Price = price;
    }

    public ProductId ProductId { get; init; }
    public OrderId OrderId { get; init; }
    public OrderItemQuantity Quantity { get; init; }
    public ProductPrice Price { get; set; }

    public static OrderItem Create(Product product, OrderId orderId, OrderItemQuantity quantity, ProductPrice price) =>
        new(OrderItemId.Create(Guid.NewGuid()).Value, product.Id, orderId, quantity, price);
}