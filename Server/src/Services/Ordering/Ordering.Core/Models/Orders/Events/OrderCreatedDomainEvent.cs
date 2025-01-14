using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.Domain.Interfaces;

namespace Ordering.Core.Models.Orders.Events;

public record OrderCreatedDomainEvent(OrderId OrderId) : IDomainEvent;