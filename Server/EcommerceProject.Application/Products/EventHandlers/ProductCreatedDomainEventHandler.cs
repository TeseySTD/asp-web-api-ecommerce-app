using System;
using EcommerceProject.Core.Models.Products.Events;
using MediatR;

namespace EcommerceProject.Application.Products.EventHandlers;

public class ProductCreatedDomainEventHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
