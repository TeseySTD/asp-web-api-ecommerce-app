using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Orders.Events;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Ordering.Core.Models.Orders;

public class Order : AggregateRoot<OrderId>
{
    private Order()
    {
    }

    private Order(OrderId id, CustomerId customerId, OrderStatus status, List<OrderItem> orderItems, Payment payment,
        Address destinationAddress) : base(id)
    {
        CustomerId = customerId;
        Status = status;
        _orderItems = orderItems ?? new List<OrderItem>();
        Payment = payment;
        DestinationAddress = destinationAddress;
    }

    public CustomerId CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public Payment Payment { get; private set; }
    public Address DestinationAddress { get; private set; }

    public OrderStatus Status { get; private set; }

    private List<OrderItem> _orderItems = new List<OrderItem>();

    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public decimal TotalPrice => _orderItems.Sum(o => o.Price.Value * o.Quantity.Value);

    public static Order Create(CustomerId customerId, Payment payment, Address destinationAddress,
        List<OrderItem>? orderItems = null, OrderId? id = null, OrderStatus status = OrderStatus.NotStarted)
    {
        orderItems ??= new List<OrderItem>();
        id ??= OrderId.Create(Guid.NewGuid()).Value;
        var order = new Order()
        {
            Id = id,
            CustomerId = customerId,
            Status = status,
            _orderItems = orderItems ?? new List<OrderItem>(),
            Payment = payment,
            DestinationAddress = destinationAddress
        };
        order.AddDomainEvent(new OrderCreatedDomainEvent(order));
        return order;
    }

    public static Order Create(CustomerId customerId, Payment payment, Address destinationAddress,
        OrderId? id = null)
    {
        return Create(customerId, payment, destinationAddress, null, id);
    }

    public void AddOrderItem(OrderItem orderItem)
    {
        _orderItems.Add(orderItem);
    }

    public void Update(Payment payment, Address destinationAddress)
    {
        Payment = payment;
        DestinationAddress = destinationAddress;

        AddDomainEvent(new OrderUpdatedDomainEvent(this));
    }

    public void Approve(IEnumerable<OrderItem> orderItems)
    {
        _orderItems.AddRange(orderItems);
        Status = OrderStatus.InProgress;
    }

    public void Cancel() => Status = OrderStatus.Cancelled;
    public void Complete() => Status = OrderStatus.Completed;
}

public enum OrderStatus
{
    NotStarted = 1,
    InProgress,
    Completed,
    Cancelled,
}