using EcommerceProject.Core.Common.Abstractions.Interfaces;

namespace EcommerceProject.Core.Models.Orders.Events;

public record OrderUpdatedDomainEvent(Order Order) : IDomainEvent;