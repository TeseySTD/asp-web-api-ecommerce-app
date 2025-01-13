using Catalog.Core.Models.Products.ValueObjects;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Products.Events;

public class ProductCreatedDomainEvent : IDomainEvent
{
    public ProductId ProductId { get; set; }
    
    public ProductCreatedDomainEvent(ProductId productId)
    {
        ProductId = productId;
    }   
}
