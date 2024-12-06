using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Orders.Events;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Core.Models.Orders;

public class Order : AggregateRoot<OrderId>
{
    private Order():base(default!){}
    private Order(OrderId id, UserId userId, OrderStatus status, ICollection<OrderItem> orderItems, Payment payment,
        Address destinationAddress) : base(id)
    {
        UserId = userId;
        Status = status;
        OrderItems = orderItems;
        Payment = payment;
        DestinationAddress = destinationAddress;
    }

    public UserId UserId { get; private set; }
    public DateTime OrderDate { get; } = DateTime.Now;
    public Payment Payment { get; private set; }
    public Address DestinationAddress { get; private set; }

    public OrderStatus Status { get; private set; }

    public ICollection<OrderItem> OrderItems { get; private set; }

    public static Order Create(UserId userId, Payment payment, Address destinationAddress,
        ICollection<OrderItem>? orderItems = null, OrderId? id = null, OrderStatus status = OrderStatus.NotStarted)
    {
        orderItems = orderItems ?? new List<OrderItem>();
        id ??= OrderId.Create(Guid.NewGuid());
        var order = new Order(id, userId, status, orderItems, payment, destinationAddress);
        order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id));
        return order;
    }
}

public enum OrderStatus
{
    NotStarted = 1,
    InProgress,
    Completed
}