using System;
using MediatR;

namespace EcommerceProject.Core.Abstractions.Interfaces;

public interface IDomainEvent : INotification
{
    Guid EventId => Guid.NewGuid();
    public DateTime OccurredOn => DateTime.Now;
    public string EventType => GetType().Name;
}
