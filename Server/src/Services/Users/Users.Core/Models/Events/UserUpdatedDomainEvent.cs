using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Users.Core.Models.Events;

public record UserUpdatedDomainEvent(User User) : DomainEvent;