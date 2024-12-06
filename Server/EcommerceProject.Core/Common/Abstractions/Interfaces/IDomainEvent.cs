using MediatR;

namespace EcommerceProject.Core.Common.Abstractions.Interfaces;

public interface IDomainEvent : INotification
{
    Guid EventId => Guid.NewGuid();
    public DateTime OccurredOn => DateTime.Now;
    public string EventType => GetType().Name;
}
