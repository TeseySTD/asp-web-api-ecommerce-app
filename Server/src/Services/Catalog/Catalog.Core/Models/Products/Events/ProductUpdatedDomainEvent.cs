using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Products.Events;

public record ProductUpdatedDomainEvent(Product Product) : IDomainEvent;