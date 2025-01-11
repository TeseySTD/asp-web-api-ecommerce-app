using Shared.Core.Domain.Interfaces;

namespace Shared.Core.Domain.Classes;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregate<TId>
{
    protected AggregateRoot(TId id) : base(id){}
    protected AggregateRoot(){}
    protected List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public IDomainEvent[] ClearDomainEvents()
    {
        var dequeuedEvents = _domainEvents.ToArray();
        
        _domainEvents.Clear();

        return dequeuedEvents;
    }
}
