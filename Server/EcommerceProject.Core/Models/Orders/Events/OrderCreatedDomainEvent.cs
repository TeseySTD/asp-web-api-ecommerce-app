using System;
using EcommerceProject.Core.Abstractions.Interfaces;

namespace EcommerceProject.Core.Models.Orders.Events;

public class OrderCreatedDomainEvent : IDomainEvent
{
    public Guid OrderId { get; set; }
    public OrderCreatedDomainEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}
