using System.Text.Json.Serialization;
using Shared.Core.Domain.Interfaces;

namespace Shared.Core.Domain.Classes;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregate<TId>
{
    protected AggregateRoot(TId id) : base(id){}
    protected AggregateRoot(){}
    protected List<DomainEvent> _domainEvents = new List<DomainEvent>();
    [JsonIgnore]
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public DomainEvent[] ClearDomainEvents()
    {
        var dequeuedEvents = _domainEvents.ToArray();
        
        _domainEvents.Clear();

        return dequeuedEvents;
    }
}
