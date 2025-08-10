using Shared.Core.Domain.Classes;

namespace Users.Core.Models.Events;

public record UserEmailVerifiedDomainEvent(User User) : DomainEvent;