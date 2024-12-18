using EcommerceProject.Core.Common;
using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Orders.Events;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Core.Models.Orders;

public class Order : AggregateRoot<OrderId>
{
    public Order()
    {
    }

    private Order(OrderId id, UserId userId, OrderStatus status, List<OrderItem> orderItems, Payment payment,
        Address destinationAddress) : base(id)
    {
        UserId = userId;
        Status = status;
        _orderItems = orderItems ?? new List<OrderItem>();
        Payment = payment;
        DestinationAddress = destinationAddress;
    }

    public UserId UserId { get; private set; }
    public User User { get; private set; }
    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public Payment Payment { get; private set; }
    public Address DestinationAddress { get; private set; }

    public OrderStatus Status { get; private set; }

    private List<OrderItem> _orderItems = new List<OrderItem>();

    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public decimal TotalPrice => _orderItems.Sum(o => o.Price.Value * o.Quantity.Value);

    public static Order Create(UserId userId, Payment payment, Address destinationAddress,
        List<OrderItem>? orderItems = null, OrderId? id = null, OrderStatus status = OrderStatus.NotStarted)
    {
        orderItems ??= new List<OrderItem>();
        id ??= OrderId.Create(Guid.NewGuid()).Value;
        var order = new Order()
        {
            Id = id,
            UserId = userId,
            Status = status,
            _orderItems = orderItems ?? new List<OrderItem>(),
            Payment = payment,
            DestinationAddress = destinationAddress
        };
        order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id));
        return order;
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        _orderItems.Add(orderItem);
    }

    public void Update(IEnumerable<OrderItem> orderItems, Payment payment, Address destinationAddress)
    {
        _orderItems = orderItems!.ToList();
        Payment = payment!;
        DestinationAddress = destinationAddress!;
        
        AddDomainEvent(new OrderUpdatedDomainEvent(this));
    }
}

public enum OrderStatus
{
    NotStarted = 1,
    InProgress,
    Completed
}