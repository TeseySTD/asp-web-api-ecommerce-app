using Catalog.Core.Models.Products.ValueObjects;
using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Products.Events;

public record ProductCreatedDomainEvent : DomainEvent
{
    public ProductId ProductId { get; set; }
    
    public ProductCreatedDomainEvent(ProductId productId)
    {
        ProductId = productId;
    }   
}
