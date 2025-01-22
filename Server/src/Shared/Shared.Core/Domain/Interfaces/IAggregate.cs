using Shared.Core.Domain.Classes;

namespace Shared.Core.Domain.Interfaces;

public interface IAggregate<TId> : IAggregate, IEntity<TId>
{
    
}

public interface IAggregate : IEntity
{
    IReadOnlyList<DomainEvent> DomainEvents { get; }

    DomainEvent[] ClearDomainEvents();
}
