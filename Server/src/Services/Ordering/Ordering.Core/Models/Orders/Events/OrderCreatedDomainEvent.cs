using Shared.Core.Domain.Classes;

namespace Ordering.Core.Models.Orders.Events;

public record OrderCreatedDomainEvent(Order Order) : DomainEvent;