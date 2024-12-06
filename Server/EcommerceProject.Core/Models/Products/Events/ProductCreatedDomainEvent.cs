using System;
using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Core.Models.Products.Events;

public class ProductCreatedDomainEvent : IDomainEvent
{
    public ProductId ProductId { get; set; }
    
    public ProductCreatedDomainEvent(ProductId productId)
    {
        ProductId = productId;
    }   
}
