﻿using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Ordering.Core.Models.Orders.Events;

public record OrderUpdatedDomainEvent(Order Order) : DomainEvent;