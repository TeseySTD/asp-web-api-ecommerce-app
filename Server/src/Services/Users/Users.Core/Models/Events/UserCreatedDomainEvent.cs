﻿using Shared.Core.Domain.Interfaces;
using Users.Core.Models.ValueObjects;

namespace Users.Core.Models.Events;

public record UserCreatedDomainEvent(UserId UserId) : IDomainEvent;