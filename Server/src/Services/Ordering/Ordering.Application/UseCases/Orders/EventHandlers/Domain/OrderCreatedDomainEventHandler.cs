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
    private readonly IPublishEndpoint _publishEndpoint;

    public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    Task INotificationHandler<OrderCreatedDomainEvent>.Handle(OrderCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            notification.EventType, notification.OccurredOnUtc,
            JsonSerializer.Serialize(notification,
                new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));

        var makeOrderEvent = new MakeOrderEvent(
            orderId: notification.Order.Id.Value,
            products: notification.Order.OrderItems.Select(oi =>
                new MakeOrderEventProduct(
                    oi.Product.Id.Value,
                    oi.Price.Value,
                    oi.Quantity.Value
                )
            ).ToList()
        );

        _publishEndpoint.Publish(makeOrderEvent);

        return Task.CompletedTask;
    }
}