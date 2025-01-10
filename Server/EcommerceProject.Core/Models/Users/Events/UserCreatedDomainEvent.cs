using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Core.Models.Users.Events;

public record UserCreatedDomainEvent(UserId UserId) : IDomainEvent;