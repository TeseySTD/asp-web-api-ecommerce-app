using System;
using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Core.Models.Orders.Events;

public record OrderCreatedDomainEvent(OrderId OrderId) : IDomainEvent;