using System;
using System.ComponentModel;
using EcommerceProject.Core.Abstractions.Classes;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Orders.Events;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Core.Models;

public class Order : AggregateRoot<Guid>
{
    private OrderStatus _status = OrderStatus.NotStarted;

    private Order(Guid id, Guid userId, OrderStatus status, ICollection<OrderItem> orderItems, Payment payment, Address destinationAddress) : base(id)
    {
        UserId = userId;
        Status = status;
        OrderItems = orderItems;
        Payment = payment;
        DestinationAddress = destinationAddress;
    }

    public Guid UserId { get; set; }
    public DateTime OrderDate { get; } = DateTime.Now;
    public Payment Payment{ get; set; } = default!;
    public Address DestinationAddress { get; set; } = default!;
    public OrderStatus Status
    {
        get => _status;
        set
        {
            if (!Enum.IsDefined(typeof(OrderStatus), value)) throw new ArgumentException("Invalid order status" ,nameof(Status));
            _status = value;
        }
    }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public static Order Create(Guid userId, Payment payment, Address destinationAddress, ICollection<OrderItem>? orderItems = null, Guid id = new Guid(), OrderStatus status = OrderStatus.NotStarted) {
        orderItems = orderItems ?? new List<OrderItem>();
        var order = new Order(id, userId, status, orderItems, payment, destinationAddress);
        order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id));
        return order;
    }

}

public enum OrderStatus{
    NotStarted = 1,
    InProgress,
    Completed
}