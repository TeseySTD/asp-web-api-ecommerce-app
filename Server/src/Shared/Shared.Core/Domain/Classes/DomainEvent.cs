using MediatR;

namespace Shared.Core.Domain.Classes;

public record DomainEvent : INotification
{
    Guid EventId => Guid.NewGuid();
    public DateTime OccurredOnUtc => DateTime.UtcNow;
    public string EventType => GetType().Name;
}
