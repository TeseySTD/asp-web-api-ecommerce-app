using System.Text.Json;
using Catalog.Core.Models.Products.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Product;

namespace Catalog.Application.UseCases.Products.EventHandlers.Domain;

public class ProductCreatedDomainEventHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            notification.EventType, notification.OccurredOnUtc,
            JsonSerializer.Serialize(notification,
                new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));

        var product = notification.Product;
        
        await _publishEndpoint.Publish(new ProductCreatedEvent(
            ProductId: product.Id.Value,
            Title: product.Title.Value,
            Description: product.Description.Value
        ));
    }
}