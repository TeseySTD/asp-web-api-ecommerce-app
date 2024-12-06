using System;
using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Core.Models.Orders.Events;

public class OrderCreatedDomainEvent : IDomainEvent
{
    public OrderId OrderId { get; set; }
    public OrderCreatedDomainEvent(OrderId orderId)
    {
        OrderId = orderId;
    }
}
