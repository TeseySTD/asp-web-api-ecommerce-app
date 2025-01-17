namespace EcommerceProject.Core.Common.Abstractions.Interfaces;

public interface IAggregate<TId> : IAggregate, IEntity<TId>
{
    
}

public interface IAggregate : IEntity
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    IDomainEvent[] ClearDomainEvents();
}
