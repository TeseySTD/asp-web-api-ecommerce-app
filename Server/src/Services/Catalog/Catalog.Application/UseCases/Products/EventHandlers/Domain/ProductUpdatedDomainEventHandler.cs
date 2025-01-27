using System.Text.Json;
using Catalog.Core.Models.Products.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Interfaces;
using Shared.Messaging.Events.Product;

namespace Catalog.Application.UseCases.Products.EventHandlers.Domain;

public class ProductUpdatedDomainEventHandler : INotificationHandler<ProductUpdatedDomainEvent>
{
    private readonly ILogger<ProductUpdatedDomainEventHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProductUpdatedDomainEventHandler(ILogger<ProductUpdatedDomainEventHandler> logger,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            notification.EventType, notification.OccurredOnUtc,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));

        await _publishEndpoint.Publish(new ProductUpdatedEvent(
            ProductId: notification.Product.Id.Value,
            Title: notification.Product.Title.Value,
            Description: notification.Product.Description.Value
        ));
    }
}