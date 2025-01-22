using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Products.Events;

public record ProductUpdatedDomainEvent(Product Product) : DomainEvent;