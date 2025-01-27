using System.Text.Json;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Core.Models.Orders.Events;
using Shared.Messaging.Events.Order;

namespace Ordering.Application.UseCases.Orders.EventHandlers.Domain;

public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger;

    public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    Task INotificationHandler<OrderCreatedDomainEvent>.Handle(OrderCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            notification.EventType, notification.OccurredOnUtc,
            JsonSerializer.Serialize(notification,
                new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));

        return Task.CompletedTask;
    }
}