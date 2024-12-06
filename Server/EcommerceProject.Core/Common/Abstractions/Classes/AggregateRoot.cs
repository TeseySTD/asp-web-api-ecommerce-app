using EcommerceProject.Core.Common.Abstractions.Interfaces;

namespace EcommerceProject.Core.Common.Abstractions.Classes;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregate<TId>
{
    protected AggregateRoot(TId id) : base(id){}
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
