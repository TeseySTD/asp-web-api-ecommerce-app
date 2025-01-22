using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Ordering.Core.Models.Orders.Events;

public record OrderCreatedDomainEvent(Order Order) : DomainEvent;