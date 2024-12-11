using EcommerceProject.Core.Models.Orders.Events;
using MediatR;

namespace EcommerceProject.Application.UseCases.Orders.EventHandlers;

public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    Task INotificationHandler<OrderCreatedDomainEvent>.Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
