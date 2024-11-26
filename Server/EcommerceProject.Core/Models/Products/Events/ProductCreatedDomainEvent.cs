using System;
using EcommerceProject.Core.Abstractions.Interfaces;

namespace EcommerceProject.Core.Models.Products.Events;

public class ProductCreatedDomainEvent : IDomainEvent
{
    public Guid ProductId { get; set; }
    
    public ProductCreatedDomainEvent(Guid productId)
    {
        ProductId = productId;
    }   
}
