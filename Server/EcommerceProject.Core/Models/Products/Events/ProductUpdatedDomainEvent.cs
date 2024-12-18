using EcommerceProject.Core.Common.Abstractions.Interfaces;

namespace EcommerceProject.Core.Models.Products.Events;

public record ProductUpdatedDomainEvent(Product Product) : IDomainEvent;