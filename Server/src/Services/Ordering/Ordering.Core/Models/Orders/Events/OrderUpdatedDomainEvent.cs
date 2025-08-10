using Shared.Core.Domain.Classes;

namespace Ordering.Core.Models.Orders.Events;

public record OrderUpdatedDomainEvent(Order Order) : DomainEvent;