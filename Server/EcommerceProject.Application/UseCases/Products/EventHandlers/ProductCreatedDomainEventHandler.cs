using EcommerceProject.Core.Models.Products.Events;
using MediatR;

namespace EcommerceProject.Application.UseCases.Products.EventHandlers;

public class ProductCreatedDomainEventHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"ProductCreatedDomainEvent: {notification}");
        return Task.CompletedTask;
    }
}
