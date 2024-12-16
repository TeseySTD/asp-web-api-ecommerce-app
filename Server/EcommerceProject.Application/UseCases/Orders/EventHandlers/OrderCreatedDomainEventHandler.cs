using EcommerceProject.Core.Models.Orders.Events;
using MediatR;

namespace EcommerceProject.Application.UseCases.Orders.EventHandlers;

public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    Task INotificationHandler<OrderCreatedDomainEvent>.Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("OrderCreatedDomainEventHandler");
        return Task.CompletedTask;
    }
}
